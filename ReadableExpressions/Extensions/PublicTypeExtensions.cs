using AgileObjects.ReadableExpressions.Translations;

namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NetStandardPolyfills;
    using static ExpressionExtensions;

    /// <summary>
    /// Provides a set of static extension methods for type information.
    /// </summary>
    public static class PublicTypeExtensions
    {
        /// <summary>
        /// Returns a friendly, readable version of the name of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which to retrieve a friendly, readable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A friendly, readable version of the name of the given <paramref name="type"/>.</returns>
        public static string GetFriendlyName(this Type type, Func<TranslationSettings, TranslationSettings> configuration = null)
            => GetFriendlyName(type, GetTranslationSettings(configuration));

        internal static string GetFriendlyName(this Type type, TranslationSettings translationSettings)
        {
            var buffer = new TranslationBuffer((type.FullName ?? type.ToString()).Length);

            buffer.WriteFriendlyName(type, translationSettings);

            return buffer.GetContent();
        }

        internal static void WriteFriendlyName(this TranslationBuffer buffer, Type type)
            => WriteFriendlyName(buffer, type, TranslationSettings.Default);

        internal static void WriteFriendlyName(
            this TranslationBuffer buffer,
            Type type,
            TranslationSettings settings)
        {
            if (type.FullName == null)
            {
                if (type.IsGenericType())
                {
                    // A generic open generic parameter Type:
                    buffer.WriteGenericTypeName(type, settings);
                }
                else
                {
                    // An open generic parameter Type:
                    buffer.WriteToTranslation(type.Name);
                }
                
                return;
            }

            if (type.IsArray)
            {
                buffer.WriteFriendlyName(type.GetElementType(), settings);
                buffer.WriteToTranslation("[]");
                return;
            }

            if (!type.IsGenericType())
            {
                var substitutedTypeName = type.GetSubstitutionOrNull();

                if (type.IsNested)
                {
                    buffer.WriteFriendlyName(type.DeclaringType, settings);
                    buffer.WriteToTranslation('.');
                    buffer.WriteToTranslation(substitutedTypeName ?? type.Name);
                    return;
                }

                if (substitutedTypeName != null)
                {
                    buffer.WriteToTranslation(substitutedTypeName);
                    return;
                }

                buffer.WriteTypeNamespaceIfRequired(type, settings);
                buffer.WriteToTranslation(type.Name);
                return;
            }

            Type underlyingNullableType;

            if ((underlyingNullableType = Nullable.GetUnderlyingType(type)) == null)
            {
                buffer.WriteGenericTypeName(type, settings);
                return;
            }

            buffer.WriteFriendlyName(underlyingNullableType, settings);
            buffer.WriteToTranslation('?');
        }

        private static void WriteTypeNamespaceIfRequired(
            this TranslationBuffer buffer,
            Type type,
            TranslationSettings settings)
        {
            if (!settings.FullyQualifyTypeNames || (type.Namespace == null))
            {
                return;
            }

            buffer.WriteToTranslation(type.Namespace);
            buffer.WriteToTranslation('.');
        }

        private static void WriteGenericTypeName(
            this TranslationBuffer buffer,
            Type genericType,
            TranslationSettings settings)
        {
            var typeGenericTypeArguments = genericType.GetGenericTypeArguments();

            if (!genericType.IsNested)
            {
                buffer.WriteTypeNamespaceIfRequired(genericType, settings);
                buffer.WriteClosedGenericTypeName(genericType, ref typeGenericTypeArguments, settings);
                return;
            }

            var types = new List<Type> { genericType };

            // ReSharper disable once PossibleNullReferenceException
            while (genericType.IsNested)
            {
                genericType = genericType.DeclaringType;
                types.Add(genericType);
            }

            buffer.WriteTypeNamespaceIfRequired(genericType, settings);

            for (var i = types.Count; ;)
            {
                buffer.WriteClosedGenericTypeName(types[--i], ref typeGenericTypeArguments, settings);

                if (i == 0)
                {
                    return;
                }

                buffer.WriteToTranslation('.');
            }
        }

        private static void WriteClosedGenericTypeName(
            this TranslationBuffer buffer,
            Type genericType,
            ref Type[] typeGenericTypeArguments,
            TranslationSettings settings)
        {
            var typeName = genericType.Name;

            var backtickIndex = typeName.IndexOf("`", StringComparison.Ordinal);

            if (backtickIndex == -1)
            {
                buffer.WriteToTranslation(typeName);
                return;
            }

            var numberOfParameters = int.Parse(typeName.Substring(backtickIndex + 1));

            Type[] typeArguments;

            if (numberOfParameters == typeGenericTypeArguments.Length)
            {
                typeArguments = typeGenericTypeArguments;
                goto WriteName;
            }

            switch (numberOfParameters)
            {
                case 1:
                    typeArguments = new[] { typeGenericTypeArguments[0] };
                    break;

                case 2:
                    typeArguments = new[] { typeGenericTypeArguments[0], typeGenericTypeArguments[1] };
                    break;

                default:
                    typeArguments = new Type[numberOfParameters];

                    Array.Copy(
                        typeGenericTypeArguments,
                        typeArguments,
                        numberOfParameters);

                    break;
            }

            var numberOfRemainingTypeArguments = typeGenericTypeArguments.Length - numberOfParameters;
            var typeGenericTypeArgumentsSubset = new Type[numberOfRemainingTypeArguments];

            Array.Copy(
                typeGenericTypeArguments,
                numberOfParameters,
                typeGenericTypeArgumentsSubset,
                0,
                numberOfRemainingTypeArguments);

            typeGenericTypeArguments = typeGenericTypeArgumentsSubset;

        WriteName:
            buffer.WriteGenericTypeName(genericType, numberOfParameters, typeArguments, settings);
        }

        private static void WriteGenericTypeName(
            this TranslationBuffer buffer,
            Type type,
            int numberOfParameters,
            IList<Type> typeArguments,
            TranslationSettings settings)
        {
            var isAnonType =
                type.Name.StartsWith('<') &&
               (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal)) != -1;

            if (isAnonType && (settings.AnonymousTypeNameFactory != null))
            {
                buffer.WriteToTranslation(settings.AnonymousTypeNameFactory.Invoke(type));
                return;
            }

            string typeName;

            if (isAnonType)
            {
                typeName = "AnonymousType";
            }
            else
            {
                var parameterCountIndex = type.Name.IndexOf("`" + numberOfParameters, StringComparison.Ordinal);
                typeName = type.Name.Substring(0, parameterCountIndex);
            }

            buffer.WriteToTranslation(typeName);
            buffer.WriteToTranslation('<');

            for (var i = 0; ;)
            {
                var typeArgument = typeArguments[i++];

                buffer.WriteFriendlyName(typeArgument, settings);

                if (i == typeArguments.Count)
                {
                    break;
                }

                buffer.WriteToTranslation(", ");
            }

            buffer.WriteToTranslation('>');
        }

        /// <summary>
        /// Retrieves a camel-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A camel-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInCamelCase(this Type type, Func<TranslationSettings, TranslationSettings> configuration = null)
            => GetVariableNameInCamelCase(type, GetTranslationSettings(configuration));

        internal static string GetVariableNameInCamelCase(this Type type, TranslationSettings settings)
            => GetVariableName(type, settings).ToCamelCase();

        /// <summary>
        /// Retrieves a pascal-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A pascal-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInPascalCase(this Type type, Func<TranslationSettings, TranslationSettings> configuration = null)
            => GetVariableNameInPascalCase(type, GetTranslationSettings(configuration));

        private static string GetVariableNameInPascalCase(this Type type, TranslationSettings settings)
            => GetVariableName(type, settings).ToPascalCase();

        private static string GetVariableName(Type type, TranslationSettings settings)
        {
            if (type.IsArray)
            {
                return GetVariableName(type.GetElementType(), settings) + "Array";
            }

            var typeIsEnumerable = type.IsEnumerable();
            var typeIsDictionary = typeIsEnumerable && type.IsDictionary();
            var namingType = (typeIsEnumerable && !typeIsDictionary) ? type.GetEnumerableElementType() : type;
            var variableName = GetBaseVariableName(namingType, settings);

            if (namingType.IsInterface())
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGenericType())
            {
                variableName = GetGenericTypeVariableName(variableName, namingType, settings);
            }

            variableName = RemoveLeadingNonAlphaNumerics(variableName);

            return (typeIsDictionary || !typeIsEnumerable) ? variableName : variableName.Pluralise();
        }

        private static string GetBaseVariableName(Type namingType, TranslationSettings translationSettings)
            => namingType.IsPrimitive() ? namingType.GetFriendlyName(translationSettings) : namingType.Name;

        private static string GetGenericTypeVariableName(
            string variableName,
            Type namingType,
            TranslationSettings settings)
        {
            var nonNullableType = namingType.GetNonNullableType();
            var genericTypeArguments = namingType.GetGenericTypeArguments();

            if (nonNullableType != namingType)
            {
                return "nullable" + genericTypeArguments[0].GetVariableNameInPascalCase(settings);
            }

            variableName = variableName.Substring(0, variableName.IndexOf('`'));

            variableName += genericTypeArguments
                .Project(arg => "_" + arg.GetVariableNameInPascalCase(settings))
                .Join(string.Empty);

            return variableName;
        }

        private static string RemoveLeadingNonAlphaNumerics(string value)
        {
            // Anonymous types start with non-alphanumeric characters
            while (!char.IsLetterOrDigit(value, 0))
            {
                value = value.Substring(1);
            }

            return value;
        }

        /// <summary>
        /// Determines if this <paramref name="type"/> is an enumerable Type.
        /// </summary>
        /// <param name="type">The Type for which to make the determination.</param>
        /// <returns>True if this <paramref name="type"/> is an enumerable Type, otherwise false.</returns>
        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray ||
                  (type != typeof(string) &&
                   type.IsAssignableTo(typeof(IEnumerable)));
        }

        /// <summary>
        /// Determines if this <paramref name="type"/> is a Dictionary Type.
        /// </summary>
        /// <param name="type">The Type for which to make the determination.</param>
        /// <returns>True if this <paramref name="type"/> is a Dictionary Type, otherwise false.</returns>
        public static bool IsDictionary(this Type type)
            => !GetDictionaryTypes(type).Equals(default(KeyValuePair<Type, Type>));

        /// <summary>
        /// Gets a KeyValuePair containing the key and value Types of this Dictionary <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the key and value Types.</param>
        /// <returns>A KeyValuePair containing the key and value Types of this Dictionary <paramref name="type"/>.</returns>
        public static KeyValuePair<Type, Type> GetDictionaryTypes(this Type type)
        {
            var dictionaryType = GetDictionaryType(type);

            return (dictionaryType != null)
                ? GetDictionaryTypesFrom(dictionaryType)
                : default(KeyValuePair<Type, Type>);
        }

        /// <summary>
        /// Gets the Dictionary Type of this <paramref name="type"/> - either the Dictionary Type it is, or
        /// the first IDictionary Type it implements. If this <paramref name="type"/> is not a Dictionary
        /// Type, returns null.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the Dictionary Type.</param>
        /// <returns>The Dictionary Type of this <paramref name="type"/>, or null if there is none.</returns>
        public static Type GetDictionaryType(this Type type)
        {
            if (type.IsGenericType())
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                if ((typeDefinition == typeof(Dictionary<,>)) || (typeDefinition == typeof(IDictionary<,>)))
                {
                    return type;
                }
            }

            var interfaceType = InternalEnumerableExtensions.FirstOrDefault(type
                    .GetAllInterfaces(), t => t.IsClosedTypeOf(typeof(IDictionary<,>)));

            return interfaceType;
        }

        private static KeyValuePair<Type, Type> GetDictionaryTypesFrom(Type type)
        {
            var types = type.GetGenericTypeArguments();
            return new KeyValuePair<Type, Type>(types[0], types[1]);
        }

        /// <summary>
        /// Gets the element Type for this <paramref name="enumerableType"/>.
        /// </summary>
        /// <param name="enumerableType">The enumerable Type for which to retrieve the element Type.</param>
        /// <returns>
        /// The element Type for this <paramref name="enumerableType"/>, or null if this Type is not enumerable.
        /// </returns>
        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            if (enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }

            if (enumerableType.IsGenericType())
            {
                return enumerableType.GetGenericTypeArguments().Last();
            }

            var enumerableInterfaceType = enumerableType
                .GetAllInterfaces()
                .FirstOrDefault(interfaceType => interfaceType.IsClosedTypeOf(typeof(IEnumerable<>)));

            return enumerableInterfaceType?.GetGenericTypeArguments().First() ?? typeof(object);
        }

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> can be null.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> can be null, otherwise false.</returns>
        public static bool CanBeNull(this Type type)
            => type.IsClass() || type.IsInterface() || type.IsNullableType();

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> is a Nullable{T}.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> is a Nullable{T}, otherwise false.</returns>
        public static bool IsNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// Gets the underlying non-nullable Type of this <paramref name="type"/>, or returns this
        /// <paramref name="type"/> if it is not nullable.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the underlying non-nullable Type.</param>
        /// <returns>
        /// The underlying non-nullable Type of this <paramref name="type"/>, or returns this
        /// <paramref name="type"/> if it is not nullable.
        /// </returns>
        [DebuggerStepThrough]
        public static Type GetNonNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
    }
}
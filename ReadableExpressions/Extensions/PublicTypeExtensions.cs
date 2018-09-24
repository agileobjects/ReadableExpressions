namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NetStandardPolyfills;
    using static TranslationContext;

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
            if (type.FullName == null)
            {
                // An open generic parameter Type:
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName(translationSettings) + "[]";
            }

            if (!type.IsGenericType())
            {
                var qualifiedTypeName = type.FullName.GetSubstitutionOrNull() ?? type.Name;

                if (type.IsNested)
                {
                    return type.DeclaringType.GetFriendlyName(translationSettings) + "." + qualifiedTypeName;
                }

                return qualifiedTypeName;
            }

            Type underlyingNullableType;

            if ((underlyingNullableType = Nullable.GetUnderlyingType(type)) != null)
            {
                return underlyingNullableType.GetFriendlyName(translationSettings) + "?";
            }

            return GetGenericTypeName(type, translationSettings);
        }

        private static string GetGenericTypeName(Type genericType, TranslationSettings settings)
        {
            var typeGenericTypeArguments = genericType.GetGenericTypeArguments();
            var genericTypeName = GetGenericTypeName(genericType, typeGenericTypeArguments.Length, typeGenericTypeArguments, settings);

            if (!genericType.IsNested)
            {
                return genericTypeName;
            }

            // ReSharper disable once PossibleNullReferenceException
            while (genericType.IsNested)
            {
                genericType = genericType.DeclaringType;
                var parentTypeName = genericType.Name;

                var backtickIndex = parentTypeName.IndexOf("`", StringComparison.Ordinal);

                if (backtickIndex != -1)
                {
                    var numberOfParameters = int.Parse(parentTypeName.Substring(backtickIndex + 1));

                    Type[] typeArguments;

                    if (numberOfParameters == typeGenericTypeArguments.Length)
                    {
                        typeArguments = typeGenericTypeArguments;
                    }
                    else
                    {
                        typeArguments = new Type[numberOfParameters];
                        var numberOfRemainingTypeArguments = typeGenericTypeArguments.Length - numberOfParameters;
                        var typeGenericTypeArgumentsSubset = new Type[numberOfRemainingTypeArguments];

                        Array.Copy(
                            typeGenericTypeArguments,
                            numberOfRemainingTypeArguments,
                            typeArguments,
                            0,
                            numberOfParameters);

                        Array.Copy(
                            typeGenericTypeArguments,
                            0,
                            typeGenericTypeArgumentsSubset,
                            0,
                            numberOfRemainingTypeArguments);

                        typeGenericTypeArguments = typeGenericTypeArgumentsSubset;
                    }

                    parentTypeName = GetGenericTypeName(genericType, numberOfParameters, typeArguments, settings);
                }

                genericTypeName = parentTypeName + "." + genericTypeName;
            }

            return genericTypeName;
        }

        private static string GetGenericTypeName(
            Type type,
            int numberOfParameters,
            IEnumerable<Type> typeArguments,
            TranslationSettings settings)
        {
            var anonTypeIndex = 0;

            var isAnonType =
                type.Name.StartsWith('<') &&
               ((anonTypeIndex = type.Name.IndexOf("AnonymousType", StringComparison.Ordinal)) != -1);

            if (isAnonType && (settings.AnonymousTypeNameFactory != null))
            {
                return settings.AnonymousTypeNameFactory.Invoke(type);
            }

            var typeName = type.Name;

            var typeGenericTypeArgumentFriendlyNames =
                typeArguments.Project(t => GetFriendlyName(t, settings)).Join(", ");

            typeName = typeName.Replace(
                "`" + numberOfParameters,
                "<" + typeGenericTypeArgumentFriendlyNames + ">");

            return isAnonType ? GetAnonymousTypeName(typeName, anonTypeIndex) : typeName;
        }

        private static string GetAnonymousTypeName(string typeName, int anonTypeIndex)
        {
            typeName = typeName.Substring(anonTypeIndex);

            var trimStartIndex = "AnonymousType".Length;
            var argumentsStartIndex = typeName.IndexOf('<');

            typeName = typeName.Remove(trimStartIndex, argumentsStartIndex - trimStartIndex);

            return typeName;
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

        internal static string GetVariableNameInPascalCase(this Type type, TranslationSettings settings)
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

            var interfaceType = type
                .GetAllInterfaces()
                .FirstOrDefault(t => t.IsClosedTypeOf(typeof(IDictionary<,>)));

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
        {
            return type.IsClass() || type.IsInterface() || type.IsNullableType();
        }

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> is a Nullable{T}.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> is a Nullable{T}, otherwise false.</returns>
        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

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
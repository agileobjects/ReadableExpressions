namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using NetStandardPolyfills;
    using static ExpressionExtensions;

    public static partial class PublicTypeExtensions
    {
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

            if (namingType.IsInterface() && variableName.StartsWith('I'))
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGenericType())
            {
                variableName = GetGenericTypeVariableName(namingType, settings);
            }

            variableName = RemoveLeadingNonAlphaNumerics(variableName);

            return (typeIsDictionary || !typeIsEnumerable) ? variableName : variableName.Pluralise();
        }

        private static string GetBaseVariableName(Type namingType, TranslationSettings translationSettings)
            => namingType.IsPrimitive() ? namingType.GetFriendlyName(translationSettings) : namingType.Name;

        private static string GetGenericTypeVariableName(Type namingType, TranslationSettings settings)
        {
            var nonNullableType = namingType.GetNonNullableType();
            var genericTypeArguments = namingType.GetGenericTypeArguments();

            if (nonNullableType != namingType)
            {
                return "nullable" + genericTypeArguments[0].GetVariableNameInPascalCase(settings);
            }

            var writer = new GenericVariableNameWriter(settings);
            writer.WriteGenericTypeName(namingType);
            
            return writer.TypeName;
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

        private class GenericVariableNameWriter : GenericTypeNameWriterBase
        {
            private readonly TranslationSettings _settings;

            public GenericVariableNameWriter(TranslationSettings settings)
            {
                _settings = settings;
                TypeName = string.Empty;
            }

            public string TypeName { get; private set; }

            protected override void WriteTypeName(string name)
                => TypeName += name;

            protected override void WriteInterfaceName(string name)
                => TypeName += name;

            protected override void WriteTypeArgumentNamePrefix()
                => TypeName += "_";

            protected override void WriteTypeName(Type type)
                => TypeName += type.GetVariableNameInPascalCase(_settings);

            protected override void WriteTypeArgumentNameSeparator()
                => TypeName += "_";

            protected override void WriteTypeArgumentNameSuffix()
            {
            }

            protected override void WriteNestedTypeNamesSeparator()
                => TypeName += "__";
        }
    }
}
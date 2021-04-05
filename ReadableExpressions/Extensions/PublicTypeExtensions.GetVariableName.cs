﻿namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using Translations.Formatting;
    using Translations.Reflection;
    using static ExpressionExtensions;

    public static partial class PublicTypeExtensions
    {
        /// <summary>
        /// Retrieves a camel-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A camel-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInCamelCase(
            this Type type,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            return BclTypeWrapper.For(type).GetVariableNameInCamelCase(configuration);
        }

        /// <summary>
        /// Retrieves a camel-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="IType"/> for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A camel-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInCamelCase(
            this IType type,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            return GetVariableNameInCamelCase(type, configuration.GetTranslationSettings());
        }

        internal static string GetVariableNameInCamelCase(this Type type, TranslationSettings settings)
            => BclTypeWrapper.For(type).GetVariableNameInCamelCase(settings);

        internal static string GetVariableNameInCamelCase(this IType type, TranslationSettings settings)
            => GetVariableName(type, settings).ToCamelCase();

        /// <summary>
        /// Retrieves a pascal-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A pascal-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInPascalCase(
            this Type type,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            return BclTypeWrapper.For(type).GetVariableNameInPascalCase(configuration);
        }

        /// <summary>
        /// Retrieves a pascal-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="IType"/> for which to retrieve the variable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A pascal-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInPascalCase(
            this IType type,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            return GetVariableNameInPascalCase(type, configuration.GetTranslationSettings());
        }

        /// <summary>
        /// Retrieves a pascal-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the variable name.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use for the variable naming.</param>
        /// <returns>A pascal-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInPascalCase(this Type type, TranslationSettings settings)
            => BclTypeWrapper.For(type).GetVariableNameInPascalCase(settings);

        /// <summary>
        /// Retrieves a pascal-case variable name for a variable of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="IType"/> for which to retrieve the variable name.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use for the variable naming.</param>
        /// <returns>A pascal-case variable name for a variable of this <paramref name="type"/>.</returns>
        public static string GetVariableNameInPascalCase(this IType type, TranslationSettings settings)
            => GetVariableName(type, settings).ToPascalCase();

        private static string GetVariableName(IType type, TranslationSettings settings)
        {
            if (type.IsArray)
            {
                return GetVariableName(type.ElementType, settings) + "Array";
            }

            var typeIsEnumerable = type.IsEnumerable;
            var typeIsDictionary = typeIsEnumerable && type.IsDictionary;
            var namingType = (typeIsEnumerable && !typeIsDictionary) ? type.ElementType : type;
            var variableName = GetBaseVariableName(namingType, settings);

            if (namingType.IsInterface && variableName.StartsWith('I'))
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGeneric)
            {
                variableName = GetGenericTypeVariableName(namingType, settings);
            }

            variableName = RemoveLeadingNonAlphaNumerics(variableName);

            return (typeIsDictionary || !typeIsEnumerable) ? variableName : variableName.Pluralise();
        }

        private static string GetBaseVariableName(IType namingType, TranslationSettings translationSettings)
        {
            return namingType.IsPrimitive
                ? namingType.GetFriendlyName(translationSettings, NullTranslationFormatter.Instance)
                : namingType.Name;
        }

        private static string GetGenericTypeVariableName(IType namingType, TranslationSettings settings)
        {
            var genericTypeArguments = namingType.GenericTypeArguments;

            if (namingType.IsNullable)
            {
                return "nullable" + genericTypeArguments[0].GetVariableNameInPascalCase(settings);
            }

            var writer = new GenericVariableNameWriter(settings);
            writer.WriteGenericTypeName(namingType, genericTypeArguments);

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

            protected override void WriteTypeName(IType type)
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
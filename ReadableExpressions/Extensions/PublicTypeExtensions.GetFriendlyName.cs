namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using NetStandardPolyfills;
    using Translations;
    using Translations.Formatting;
    using static ExpressionExtensions;
    using static Translations.Formatting.TokenType;

    /// <summary>
    /// Provides a set of static extension methods for type information.
    /// </summary>
    public static partial class PublicTypeExtensions
    {
        /// <summary>
        /// Returns a friendly, readable version of the name of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which to retrieve a friendly, readable name.</param>
        /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
        /// <returns>A friendly, readable version of the name of the given <paramref name="type"/>.</returns>
        public static string GetFriendlyName(this Type type, Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            var settings = configuration.GetTranslationSettings();

            return GetFriendlyName(type, settings, settings.Formatter);
        }

        internal static string GetFriendlyName(
            this Type type, 
            TranslationSettings translationSettings,
            ITranslationFormatter formatter)
        {
            var writer = new TranslationWriter(
                formatter,
                translationSettings.Indent,
                (type.FullName ?? type.ToString()).Length);

            writer.WriteFriendlyName(type, translationSettings);

            return writer.GetContent();
        }

        internal static void WriteFriendlyName(
            this TranslationWriter writer,
            Type type,
            TranslationSettings settings)
        {
            if (type.FullName == null)
            {
                if (type.IsGenericType())
                {
                    // A generic open generic parameter Type:
                    writer.WriteGenericTypeName(type, settings);
                }
                else
                {
                    // An open generic parameter Type:
                    writer.WriteToTranslation(type.Name, InterfaceName);
                }

                return;
            }

            if (type.IsArray)
            {
                writer.WriteFriendlyName(type.GetElementType(), settings);
                writer.WriteToTranslation("[]");
                return;
            }

            if (!type.IsGenericType())
            {
                var substitutedTypeName = type.GetSubstitutionOrNull();

                if (type.IsNested)
                {
                    writer.WriteFriendlyName(type.DeclaringType, settings);
                    writer.WriteDotToTranslation();

                    if (!WriteSubstituteToTranslation(substitutedTypeName, writer))
                    {
                        writer.WriteTypeName(type);
                    }

                    return;
                }

                if (WriteSubstituteToTranslation(substitutedTypeName, writer))
                {
                    return;
                }

                writer.WriteTypeNamespaceIfRequired(type, settings);
                writer.WriteTypeName(type);
                return;
            }

            Type underlyingNullableType;

            if ((underlyingNullableType = Nullable.GetUnderlyingType(type)) == null)
            {
                writer.WriteGenericTypeName(type, settings);
                return;
            }

            writer.WriteFriendlyName(underlyingNullableType, settings);
            writer.WriteToTranslation('?');
        }

        private static bool WriteSubstituteToTranslation(
            string substitutedTypeName,
            TranslationWriter writer)
        {
            if (substitutedTypeName == null)
            {
                return false;
            }

            writer.WriteKeywordToTranslation(substitutedTypeName);
            return true;
        }

        private static void WriteTypeNamespaceIfRequired(
            this TranslationWriter writer,
            Type type,
            TranslationSettings settings)
        {
            if (!settings.FullyQualifyTypeNames || (type.Namespace == null))
            {
                return;
            }

            writer.WriteToTranslation(type.Namespace);
            writer.WriteDotToTranslation();
        }

        private static void WriteTypeName(this TranslationWriter writer, Type type)
        {
            var tokenType = type.IsClass() ? TypeName : InterfaceName;
            writer.WriteToTranslation(type.Name, tokenType);
        }

        private static void WriteGenericTypeName(
            this TranslationWriter writer,
            Type genericType,
            TranslationSettings settings)
        {
            new GenericTypeNameWriter(writer, settings).WriteGenericTypeName(genericType);
        }

        private class GenericTypeNameWriter : GenericTypeNameWriterBase
        {
            private readonly TranslationWriter _writer;
            private readonly TranslationSettings _settings;

            public GenericTypeNameWriter(TranslationWriter writer, TranslationSettings settings)
            {
                _writer = writer;
                _settings = settings;
            }

            protected override void WriteTypeName(string name)
                => _writer.WriteTypeNameToTranslation(name);

            protected override void WriteInterfaceName(string name)
                => _writer.WriteToTranslation(name, InterfaceName);

            protected override bool TryWriteCustomAnonTypeName(Type anonType)
            {
                if (_settings.AnonymousTypeNameFactory == null)
                {
                    return base.TryWriteCustomAnonTypeName(anonType);
                }

                _writer.WriteToTranslation(_settings.AnonymousTypeNameFactory.Invoke(anonType));
                return true;
            }

            protected override void WriteTypeArgumentNamePrefix()
                => _writer.WriteToTranslation('<');

            protected override void WriteTypeName(Type type)
                => _writer.WriteFriendlyName(type, _settings);

            protected override void WriteTypeArgumentNameSeparator()
                => _writer.WriteToTranslation(", ");

            protected override void WriteTypeArgumentNameSuffix()
                => _writer.WriteToTranslation('>');

            protected override void WriteTypeNamePrefix(Type genericType)
                => _writer.WriteTypeNamespaceIfRequired(genericType, _settings);

            protected override void WriteNestedTypeNamesSeparator()
                => _writer.WriteDotToTranslation();
        }
    }
}
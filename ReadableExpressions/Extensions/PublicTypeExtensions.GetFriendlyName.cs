namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using NetStandardPolyfills;
    using Translations;
    using static ExpressionExtensions;
    using static Translations.TokenType;

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
                    buffer.WriteDotToTranslation();

                    if (!WriteSubstituteToTranslation(substitutedTypeName, buffer))
                    {
                        buffer.WriteTypeName(type);
                    }

                    return;
                }

                if (WriteSubstituteToTranslation(substitutedTypeName, buffer))
                {
                    return;
                }

                buffer.WriteTypeNamespaceIfRequired(type, settings);
                buffer.WriteTypeName(type);
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

        private static bool WriteSubstituteToTranslation(
            string substitutedTypeName,
            TranslationBuffer buffer)
        {
            if (substitutedTypeName == null)
            {
                return false;
            }

            buffer.WriteToTranslation(substitutedTypeName, Keyword);
            return true;
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
            buffer.WriteDotToTranslation();
        }

        private static void WriteTypeName(this TranslationBuffer buffer, Type type)
        {
            var tokenType = type.IsClass() ? TypeName : InterfaceName;
            buffer.WriteToTranslation(type.Name, tokenType);
        }

        private static void WriteGenericTypeName(
            this TranslationBuffer buffer,
            Type genericType,
            TranslationSettings settings)
        {
            new GenericTypeNameWriter(buffer, settings).WriteGenericTypeName(genericType);
        }

        private class GenericTypeNameWriter : GenericTypeNameWriterBase
        {
            private readonly TranslationBuffer _buffer;
            private readonly TranslationSettings _settings;

            public GenericTypeNameWriter(TranslationBuffer buffer, TranslationSettings settings)
            {
                _buffer = buffer;
                _settings = settings;
            }

            protected override void WriteTypeName(string name)
                => _buffer.WriteToTranslation(name, TypeName);

            protected override void WriteInterfaceName(string name)
                => _buffer.WriteToTranslation(name, InterfaceName);

            protected override bool TryWriteCustomAnonTypeName(Type anonType)
            {
                if (_settings.AnonymousTypeNameFactory == null)
                {
                    return base.TryWriteCustomAnonTypeName(anonType);
                }

                _buffer.WriteToTranslation(_settings.AnonymousTypeNameFactory.Invoke(anonType));
                return true;
            }

            protected override void WriteTypeArgumentNamePrefix()
                => _buffer.WriteToTranslation('<');

            protected override void WriteTypeName(Type type)
                => _buffer.WriteFriendlyName(type, _settings);

            protected override void WriteTypeArgumentNameSeparator()
                => _buffer.WriteToTranslation(", ");

            protected override void WriteTypeArgumentNameSuffix()
                => _buffer.WriteToTranslation('>');

            protected override void WriteTypeNamePrefix(Type genericType)
                => _buffer.WriteTypeNamespaceIfRequired(genericType, _settings);

            protected override void WriteNestedTypeNamesSeparator()
                => _buffer.WriteDotToTranslation();
        }
    }
}
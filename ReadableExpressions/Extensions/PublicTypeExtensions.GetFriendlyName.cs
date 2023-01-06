namespace AgileObjects.ReadableExpressions.Extensions;

using System;
using Translations;
using Translations.Formatting;
using Translations.Reflection;
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
    public static string GetFriendlyName(
        this Type type,
        Func<ITranslationSettings, ITranslationSettings> configuration = null)
    {
        return ClrTypeWrapper.For(type).GetFriendlyName(configuration);
    }

    /// <summary>
    /// Returns a friendly, readable version of the name of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="IType"/> for which to retrieve a friendly, readable name.</param>
    /// <param name="configuration">The configuration to use for the variable naming, if required.</param>
    /// <returns>A friendly, readable version of the name of the given <paramref name="type"/>.</returns>
    public static string GetFriendlyName(
        this IType type,
        Func<ITranslationSettings, ITranslationSettings> configuration = null)
    {
        var settings = configuration.GetTranslationSettings();

        return type.GetFriendlyName(settings, settings.Formatter);
    }

    internal static string GetFriendlyName(
        this IType type,
        TranslationSettings translationSettings,
        ITranslationFormatter formatter)
    {
        var writer = new TranslationWriter(
            formatter,
            translationSettings.Indent);

        writer.WriteFriendlyName(type, translationSettings);

        return writer.GetContent();
    }

    internal static void WriteFriendlyName(
        this TranslationWriter writer,
        IType type,
        TranslationSettings settings)
    {
        if (type.IsArray)
        {
            writer.WriteFriendlyName(type.ElementType, settings);
            writer.WriteToTranslation("[]");
            return;
        }

        if (type.FullName == null)
        {
            if (type.IsGeneric)
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

        if (!type.IsGeneric)
        {
            var substitutedTypeName = type.GetKeywordOrNull();

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

        if (type.IsNullable)
        {
            writer.WriteFriendlyName(type.NonNullableUnderlyingType, settings);
            writer.WriteToTranslation('?');
            return;
        }

        writer.WriteGenericTypeName(type, settings);
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
        IType type,
        TranslationSettings settings)
    {
        if (!settings.FullyQualifyTypeNames || (type.Namespace == null))
        {
            return;
        }

        writer.WriteToTranslation(type.Namespace);
        writer.WriteDotToTranslation();
    }

    private static void WriteTypeName(this TranslationWriter writer, IType type)
    {
        var tokenType = type.IsClass ? TypeName : InterfaceName;
        writer.WriteToTranslation(type.Name, tokenType);
    }

    private static void WriteGenericTypeName(
        this TranslationWriter writer,
        IType genericType,
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

        protected override bool TryWriteCustomAnonTypeName(IType anonType)
        {
            if (_settings.AnonymousTypeNameFactory == null)
            {
                return base.TryWriteCustomAnonTypeName(anonType);
            }

            var anonTypeNameTranslation = _settings
                .AnonymousTypeNameFactory.Invoke(anonType.AsType());

            _writer.WriteToTranslation(anonTypeNameTranslation);
            return true;
        }

        protected override void WriteTypeArgumentNamePrefix()
            => _writer.WriteToTranslation('<');

        protected override void WriteTypeArgumentName(IType type)
            => _writer.WriteFriendlyName(type, _settings);

        protected override void WriteTypeArgumentNameSeparator()
            => _writer.WriteToTranslation(", ");

        protected override void WriteTypeArgumentNameSuffix()
            => _writer.WriteToTranslation('>');

        protected override void WriteTypeNamePrefix(IType genericType)
            => _writer.WriteTypeNamespaceIfRequired(genericType, _settings);

        protected override void WriteNestedTypeNamesSeparator()
            => _writer.WriteDotToTranslation();
    }
}
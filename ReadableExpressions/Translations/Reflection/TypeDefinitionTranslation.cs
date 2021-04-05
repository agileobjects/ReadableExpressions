namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
#if NETSTANDARD1_0
    using System.Reflection;
#endif
    using NetStandardPolyfills;
    using Interfaces;

    internal class TypeDefinitionTranslation : ITranslatable
    {
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly ITranslatable _typeNameTranslatable;

        public TypeDefinitionTranslation(Type type, ITranslationSettings settings)
        {
            _accessibility = GetAccessibility(type);
            _modifiers = GetModifiers(type);
            _typeNameTranslatable = new TypeNameTranslation(type, settings);

            TranslationSize =
                _accessibility.Length +
                _modifiers.Length +
                _typeNameTranslatable.TranslationSize;

            FormattingSize =
                settings.GetKeywordFormattingSize() + // <- For modifiers
                _typeNameTranslatable.FormattingSize;
        }

        private static string GetAccessibility(Type type)
        {
            if (type.IsPublic())
            {
                return "public ";
            }

            if (!type.IsNested)
            {
                return "internal ";
            }
#if NETSTANDARD1_0
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsNestedPublic)
#else
            if (type.IsNestedPublic)
#endif
            {
                return "public ";
            }
#if NETSTANDARD1_0
            if (typeInfo.IsNestedAssembly)
#else
            if (type.IsNestedAssembly)
#endif
            {
                return "internal ";
            }
#if NETSTANDARD1_0
            if (typeInfo.IsNestedFamORAssem)
#else
            if (type.IsNestedFamORAssem)
#endif
            {
                return "protected internal ";
            }
#if NETSTANDARD1_0
            if (typeInfo.IsNestedFamily)
#else
            if (type.IsNestedFamily)
#endif
            {
                return "protected ";
            }
#if NETSTANDARD1_0
            if (typeInfo.IsNestedPrivate)
#else
            if (type.IsNestedPrivate)
#endif
            {
                return "private ";
            }

            return string.Empty;
        }

        private static string GetModifiers(Type type)
        {
            if (type.IsInterface())
            {
                return "interface ";
            }

            if (type.IsValueType())
            {
                return "struct ";
            }

            if (type.IsAbstract())
            {
                return type.IsSealed() ? "static class " : "abstract class ";
            }

            if (type.IsSealed())
            {
                return "sealed class ";
            }

            return "class ";
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _typeNameTranslatable.GetIndentSize();

        public int GetLineCount() => _typeNameTranslatable.GetLineCount();

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);

            _typeNameTranslatable.WriteTo(writer);
        }
    }
}

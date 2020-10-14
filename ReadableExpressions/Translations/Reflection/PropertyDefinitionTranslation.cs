namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
#if NETSTANDARD
    using NetStandardPolyfills;
#endif
    using static MethodTranslationHelpers;

    internal class PropertyDefinitionTranslation : ITranslatable
    {
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly ITranslatable _declaringTypeNameTranslation;
        private readonly ITranslatable _propertyTypeTranslation;
        private readonly string _propertyName;
        private readonly ITranslatable[] _accessorTranslations;

        public PropertyDefinitionTranslation(
            PropertyInfo property,
            TranslationSettings settings)
            : this(property, property.GetAccessors(nonPublic: true), settings)
        {
        }

        public PropertyDefinitionTranslation(
            PropertyInfo property,
            MethodInfo accessor,
            TranslationSettings settings)
            : this(property, new[] { accessor }, settings)
        {
        }

        public PropertyDefinitionTranslation(
            PropertyInfo property,
            IList<MethodInfo> accessors,
            TranslationSettings settings)
        {
            _accessibility = GetAccessibility(property);
            _modifiers = GetModifiers(accessors[0], settings);

            _propertyTypeTranslation =
                new TypeNameTranslation(property.PropertyType, settings);

            _propertyName = property.Name;

            var translationSize =
                _accessibility.Length +
                _modifiers.Length +
                _propertyTypeTranslation.TranslationSize +
                _propertyName.Length;

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            var formattingSize =
                keywordFormattingSize + // <- For modifiers
                _propertyTypeTranslation.FormattingSize;

            if (property.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(property.DeclaringType, settings);

                translationSize += _declaringTypeNameTranslation.TranslationSize + ".".Length;
                formattingSize += _declaringTypeNameTranslation.FormattingSize;
            }

            _accessorTranslations = new ITranslatable[accessors.Count];

            for (var i = 0; i < accessors.Count; ++i)
            {
                var accessorTranslation =
                    new PropertyAccessorDefinitionTranslation(this, accessors[i], settings);

                translationSize += accessorTranslation.TranslationSize;
                formattingSize += accessorTranslation.FormattingSize;

                _accessorTranslations[i] = accessorTranslation;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _propertyTypeTranslation.GetIndentSize();

        public int GetLineCount() => _propertyTypeTranslation.GetLineCount();

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);

            _propertyTypeTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_propertyName);
            writer.WriteToTranslation(" { ");

            foreach (var accessorTranslation in _accessorTranslations)
            {
                accessorTranslation.WriteTo(writer);
            }

            writer.WriteToTranslation("}");
        }

        private class PropertyAccessorDefinitionTranslation : ITranslatable
        {
            private readonly string _accessor;

            public PropertyAccessorDefinitionTranslation(
                PropertyDefinitionTranslation parent,
                MethodInfo accessor,
                TranslationSettings settings)
            {
                var accessibility = GetAccessibility(accessor, settings);
                _accessor = accessor.ReturnType != typeof(void) ? "get" : "set";

                TranslationSize = _accessor.Length + "; ".Length;
                FormattingSize = settings.GetKeywordFormattingSize();

                if (accessibility == parent._accessibility)
                {
                    return;
                }

                _accessor = accessibility + _accessor;
                TranslationSize += accessibility.Length;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteKeywordToTranslation(_accessor);
                writer.WriteToTranslation("; ");
            }
        }
    }
}
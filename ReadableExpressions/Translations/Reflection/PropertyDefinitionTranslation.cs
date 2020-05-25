namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Interfaces;
    using static MethodTranslationHelpers;

    internal class PropertyDefinitionTranslation : ITranslatable
    {
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly TypeNameTranslation _propertyTypeTranslation;
        private readonly TypeNameTranslation _declaringTypeNameTranslation;
        private readonly string _propertyName;
        private readonly string _accessor;

        public PropertyDefinitionTranslation(
            PropertyInfo property,
            MethodInfo accessor,
            ITranslationSettings settings)
        {
            _accessibility = GetAccessibility(accessor);
            _modifiers = GetModifiers(accessor);

            _propertyTypeTranslation =
                new TypeNameTranslation(property.PropertyType, settings);

            if (accessor.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(accessor.DeclaringType, settings);
            }

            _propertyName = property.Name;
            _accessor = (accessor.ReturnType != typeof(void)) ? "get" : "set";

            TranslationSize =
                _accessibility.Length +
                _modifiers.Length +
                _propertyTypeTranslation.TranslationSize +
                _propertyName.Length +
                _accessor.Length + 7;

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            FormattingSize =
                keywordFormattingSize + // <- For modifiers
                _propertyTypeTranslation.FormattingSize +
                keywordFormattingSize; // <- for accessor

            if (_declaringTypeNameTranslation != null)
            {
                TranslationSize += _declaringTypeNameTranslation.TranslationSize + 1;
                FormattingSize += _declaringTypeNameTranslation.FormattingSize;
            }
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
            writer.WriteKeywordToTranslation(_accessor);
            writer.WriteToTranslation("; }");
        }
    }
}
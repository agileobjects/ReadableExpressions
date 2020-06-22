namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Interfaces;
    using static MethodTranslationHelpers;

    internal class ConstructorDefinitionTranslation : ITranslatable
    {
        private readonly string _accessibility;
        private readonly TypeNameTranslation _typeNameTranslation;
        private readonly ITranslatable _parametersTranslation;

        public ConstructorDefinitionTranslation(
            ConstructorInfo ctor,
            TranslationSettings settings)
        {
            var ctorMethod = new CtorInfoWrapper(ctor);

            _accessibility = GetAccessibility(ctorMethod);
            _typeNameTranslation = new TypeNameTranslation(ctor.DeclaringType, settings);
            _parametersTranslation = new ParameterSetDefinitionTranslation(ctorMethod, settings);

            TranslationSize =
                _typeNameTranslation.TranslationSize +
                _parametersTranslation.TranslationSize;

            FormattingSize =
                settings.GetKeywordFormattingSize() + // <- for modifiers
                _typeNameTranslation.FormattingSize +
                _parametersTranslation.FormattingSize;
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _parametersTranslation.GetIndentSize();

        public int GetLineCount() => _parametersTranslation.GetLineCount() + 1;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility);

            _typeNameTranslation.WriteTo(writer);
            _parametersTranslation.WriteTo(writer);
        }
    }
}

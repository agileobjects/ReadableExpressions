namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
    using static MethodTranslationHelpers;

    internal class OperatorDefinitionTranslation : ITranslatable
    {
        private readonly string _modifiers;
        private readonly ITranslatable _returnTypeTranslation;
        private readonly ITranslatable _parametersTranslation;

        public OperatorDefinitionTranslation(
            MethodInfo @operator,
            string typeKeyword,
            TranslationSettings settings)
        {
            _modifiers = GetAccessibility(@operator, settings) + "static " + typeKeyword + " operator ";

            _returnTypeTranslation =
                new TypeNameTranslation(@operator.ReturnType, settings);

            _parametersTranslation = new ParameterSetDefinitionTranslation(@operator, settings);

            TranslationSize =
                _modifiers.Length +
                _returnTypeTranslation.TranslationSize +
                _parametersTranslation.TranslationSize;

            FormattingSize =
                settings.GetKeywordFormattingSize() + // <- For modifiers
                _returnTypeTranslation.FormattingSize +
                _parametersTranslation.FormattingSize;
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _parametersTranslation.GetIndentSize();

        public int GetLineCount() => _parametersTranslation.GetLineCount() + 1;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_modifiers);

            _returnTypeTranslation.WriteTo(writer);
            _parametersTranslation.WriteTo(writer);
        }
    }
}
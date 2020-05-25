namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    internal interface ITranslatable
    {
        int TranslationSize { get; }

        int FormattingSize { get; }

        int GetIndentSize();

        int GetLineCount();

        void WriteTo(TranslationWriter writer);
    }
}
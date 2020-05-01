namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    internal interface ITranslatable
    {
        int TranslationSize { get; }
        
        int FormattingSize { get; }

        void WriteTo(TranslationBuffer buffer);
    }
}
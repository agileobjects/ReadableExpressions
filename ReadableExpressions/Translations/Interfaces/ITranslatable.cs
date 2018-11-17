namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    internal interface ITranslatable
    {
        int EstimatedSize { get; }

        void WriteTo(TranslationBuffer buffer);
    }
}
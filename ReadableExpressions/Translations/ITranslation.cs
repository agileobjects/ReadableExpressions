namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface ITranslation
    {
        int EstimatedSize { get; }

        void WriteTo(ITranslationContext context);
    }
}
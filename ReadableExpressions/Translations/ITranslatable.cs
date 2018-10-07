namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface ITranslatable
    {
        int EstimatedSize { get; }

        void WriteTo(ITranslationContext context);
    }
}
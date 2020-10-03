namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    internal interface IInitializerSetTranslation : ITranslatable
    {
        int Count { get; }

        bool IsLongTranslation { get; set; }
    }
}
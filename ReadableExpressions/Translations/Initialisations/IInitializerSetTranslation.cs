namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using Interfaces;

    internal interface IInitializerSetTranslation : ITranslatable
    {
        int Count { get; }

        bool IsLongTranslation { get; set; }
    }
}
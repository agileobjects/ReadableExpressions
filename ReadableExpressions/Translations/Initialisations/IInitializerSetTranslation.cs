namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using Interfaces;

    internal interface IInitializerSetTranslation : ITranslatable
    {
        bool IsLongTranslation { get; set; }
    }
}
namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

internal interface IInitializerSetTranslation : ITranslation
{
    ITranslation Parent { set; }

    int Count { get; }
}
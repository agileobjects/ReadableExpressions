namespace AgileObjects.ReadableExpressions.Translations;

internal interface IPotentialGotoTranslation : ITranslation
{
    bool HasGoto { get; }
}
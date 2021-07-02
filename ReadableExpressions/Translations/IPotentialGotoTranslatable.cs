namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IPotentialGotoTranslatable : ITranslatable
    {
        bool HasGoto { get; }
    }
}
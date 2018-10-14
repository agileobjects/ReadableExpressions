namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IPotentialMultiStatementTranslatable : ITranslatable
    {
        bool IsMultiStatement { get; }
    }
}
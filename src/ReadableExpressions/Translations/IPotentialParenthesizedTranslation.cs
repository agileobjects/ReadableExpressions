namespace AgileObjects.ReadableExpressions.Translations;

internal interface IPotentialParenthesizedTranslation : ITranslation
{
    bool Parenthesize { get; }
}
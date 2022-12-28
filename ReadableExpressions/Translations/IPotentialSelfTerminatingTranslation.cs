namespace AgileObjects.ReadableExpressions.Translations;

internal interface IPotentialSelfTerminatingTranslation : ITranslation
{
    bool IsTerminated { get; }
}
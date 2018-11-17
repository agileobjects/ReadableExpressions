namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    internal interface IPotentialSelfTerminatingTranslatable
    {
        bool IsTerminated { get; }
    }
}
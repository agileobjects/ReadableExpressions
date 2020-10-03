namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IPotentialSelfTerminatingTranslatable
    {
        bool IsTerminated { get; }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IPotentialSelfTerminatingTranslatable : ITranslatable
    {
        bool IsTerminated { get; }
    }
}
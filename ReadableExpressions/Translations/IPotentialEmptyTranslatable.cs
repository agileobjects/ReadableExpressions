namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IPotentialEmptyTranslatable : ITranslatable
    {
        bool IsEmpty { get; }
    }
}
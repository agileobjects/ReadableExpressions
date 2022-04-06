namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IParameterTranslation : ITranslation
    {
        string Name { get; }

        ITranslation WithTypeNames(ITranslationContext context);

        void WithoutTypeNames(ITranslationContext context);
    }
}
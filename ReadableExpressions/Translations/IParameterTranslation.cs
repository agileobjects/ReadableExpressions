namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IParameterTranslation : ITranslation
    {
        string Name { get; }

        void WithTypeNames(ITranslationContext context);
        
        void WithoutTypeNames(ITranslationContext context);
    }
}
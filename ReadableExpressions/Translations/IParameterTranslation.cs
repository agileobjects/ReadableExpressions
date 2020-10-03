namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface IParameterTranslation : ITranslation
    {
        void WithTypeNames(ITranslationContext context);
        
        void WithoutTypeNames(ITranslationContext context);
    }
}
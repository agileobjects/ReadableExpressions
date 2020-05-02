namespace AgileObjects.ReadableExpressions.Translations
{
    using Interfaces;

    internal interface IParameterTranslation : ITranslation
    {
        void WithTypeNames(ITranslationContext context);
        
        void WithoutTypeNames(ITranslationContext context);
    }
}
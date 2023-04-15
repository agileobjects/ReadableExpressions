namespace AgileObjects.ReadableExpressions.Translations;

internal interface IParameterTranslation : INodeTranslation
{
    string Name { get; }

    INodeTranslation WithTypeNames(ITranslationContext context);

    void WithoutTypeNames(ITranslationContext context);
}
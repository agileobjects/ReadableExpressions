namespace AgileObjects.ReadableExpressions.Translations;

using Reflection;

internal abstract class NewingTranslationBase
{
    protected NewingTranslationBase(
        NewExpression newing,
        ITranslationContext context)
    {
        Parameters = ParameterSetTranslation.For(
            new ClrCtorInfoWrapper(newing.Constructor),
            newing.Arguments,
            context);
    }

    public ExpressionType NodeType => ExpressionType.New;

    protected ParameterSetTranslation Parameters { get; }
}
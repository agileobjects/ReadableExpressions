namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal class ListInitialisationTranslation : 
    InitialisationTranslationBase<ElementInit>
{
    private ListInitialisationTranslation(
        ListInitExpression listInit,
        ITranslationContext context) : 
        base(
            ExpressionType.ListInit,
            listInit.NewExpression,
            new ListInitializerSetTranslation(listInit.Initializers, context),
            context)
    {
    }

    public static INodeTranslation For(
        ListInitExpression listInit, 
        ITranslationContext context)
    {
        if (InitHasNoInitializers(listInit.NewExpression, listInit.Initializers, context, out var newingTranslation))
        {
            return newingTranslation.WithNodeType(ExpressionType.ListInit);
        }

        return new ListInitialisationTranslation(listInit, context);
    }
}
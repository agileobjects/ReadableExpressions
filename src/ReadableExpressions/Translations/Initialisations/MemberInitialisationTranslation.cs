namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal class MemberInitialisationTranslation : 
    InitialisationTranslationBase<MemberBinding>
{
    private MemberInitialisationTranslation(
        MemberInitExpression memberInit, 
        ITranslationContext context) : 
        base(
            ExpressionType.MemberInit,
            memberInit.NewExpression,
            new MemberBindingInitializerTranslationSet(memberInit.Bindings, context),
            context)
    {
    }

    public static INodeTranslation For(
        MemberInitExpression memberInit, 
        ITranslationContext context)
    {
        if (InitHasNoInitializers(memberInit.NewExpression, memberInit.Bindings, context, out var newingTranslation))
        {
            return newingTranslation.WithNodeType(ExpressionType.MemberInit);
        }

        return new MemberInitialisationTranslation(memberInit, context);
    }
}
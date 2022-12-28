namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class ArrayInitializerSetTranslation : 
    InitializerSetTranslationBase<Expression>
{
    public ArrayInitializerSetTranslation(
        NewArrayExpression arrayInit,
        ITranslationContext context) : 
        base(arrayInit.Expressions, context)
    {
    }

    protected override ITranslation GetTranslationFor(
        Expression initializer, 
        ITranslationContext context)
    {
        return context.GetCodeBlockTranslationFor(initializer);
    }
}
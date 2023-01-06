namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class QuotedLambdaTranslation : INodeTranslation
{
    private readonly INodeTranslation _quotedLambdaTranslation;

    private QuotedLambdaTranslation(
        UnaryExpression quotedLambda,
        ITranslationContext context)
    {
        var comment = ReadableExpression.Comment("Quoted to induce a closure:");
        var quotedLambdaBlock = Expression.Block(comment, quotedLambda.Operand);

        _quotedLambdaTranslation = context.GetTranslationFor(quotedLambdaBlock);
    }

    public static INodeTranslation For(
        UnaryExpression quotedLambda,
        ITranslationContext context)
    {
        if (context.Settings.DoNotCommentQuotedLambdas)
        {
            return context
                .GetCodeBlockTranslationFor(quotedLambda.Operand)
                .WithNodeType(ExpressionType.Quote);
        }

        return new QuotedLambdaTranslation(quotedLambda, context);
    }

    public ExpressionType NodeType => ExpressionType.Quote;

    public int TranslationLength => _quotedLambdaTranslation.TranslationLength;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteNewLineToTranslation();
        _quotedLambdaTranslation.WriteTo(writer);
    }
}
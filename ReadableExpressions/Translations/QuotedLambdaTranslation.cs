namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class QuotedLambdaTranslation : ITranslation
    {
        private readonly ITranslation _quotedLambdaTranslation;

        private QuotedLambdaTranslation(UnaryExpression quotedLambda, ITranslationContext context)
        {
            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quotedLambda.Operand);

            _quotedLambdaTranslation = context
                .GetCodeBlockTranslationFor(quotedLambdaBlock)
                .WithoutBraces();
        }

        public static ITranslation For(UnaryExpression quotedLambda, ITranslationContext context)
        {
            if (context.Settings.DoNotCommentQuotedLambdas)
            {
                return context.GetCodeBlockTranslationFor(quotedLambda.Operand).WithNodeType(ExpressionType.Quote);
            }

            return new QuotedLambdaTranslation(quotedLambda, context);
        }

        public ExpressionType NodeType => ExpressionType.Quote;

        public int EstimatedSize => _quotedLambdaTranslation.EstimatedSize;

        public void WriteTo(ITranslationContext context)
        {
            context.WriteNewLineToTranslation();
            _quotedLambdaTranslation.WriteTo(context);
        }
    }
}
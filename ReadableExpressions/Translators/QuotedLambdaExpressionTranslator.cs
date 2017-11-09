namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class QuotedLambdaExpressionTranslator : ExpressionTranslatorBase
    {
        public QuotedLambdaExpressionTranslator()
            : base(ExpressionType.Quote)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var quote = (UnaryExpression)expression;

            if (context.Settings.DoNotCommentQuotedLambdas)
            {
                return context.TranslateAsCodeBlock(quote.Operand);
            }

            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quote.Operand);

            var translatedLambda = context
                .TranslateCodeBlock(quotedLambdaBlock)
                .Indented()
                .WithoutCurlyBraces();

            return Environment.NewLine + translatedLambda;
        }
    }
}
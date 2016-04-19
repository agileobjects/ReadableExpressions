namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class QuotedLambdaExpressionTranslator : ExpressionTranslatorBase
    {
        public QuotedLambdaExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Quote)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var quote = (UnaryExpression)expression;

            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quote.Operand);
            var translatedLambda = GetTranslatedExpressionBody(quotedLambdaBlock, context);

            return Environment.NewLine + translatedLambda.Indented().WithoutParentheses();
        }
    }
}
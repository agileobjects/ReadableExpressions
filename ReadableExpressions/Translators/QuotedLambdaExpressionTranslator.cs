namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Formatting;

    internal class QuotedLambdaExpressionTranslator : ExpressionTranslatorBase
    {
        public QuotedLambdaExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Quote)
        {
        }

        public override string Translate(Expression expression)
        {
            var quote = (UnaryExpression)expression;

            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quote.Operand);
            var translatedLambda = Registry.TranslateExpressionBody(quotedLambdaBlock);

            return Environment.NewLine + translatedLambda.Indented().WithoutBrackets();
        }
    }
}
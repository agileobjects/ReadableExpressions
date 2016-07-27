namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator()
            : base(ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = context.TranslateParameters(lambda.Parameters).WithParenthesesIfNecessary();
            var bodyBlock = context.TranslateCodeBlock(lambda.Body);

            var body = bodyBlock.IsASingleStatement
                ? bodyBlock.AsExpressionBody()
                : bodyBlock.WithCurlyBraces();

            return parameters + " =>" + body;
        }
    }
}
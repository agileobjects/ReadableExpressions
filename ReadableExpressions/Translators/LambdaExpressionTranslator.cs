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

            var parameters = GetTranslatedParameters(lambda.Parameters, context).WithParenthesesIfNecessary();

            var bodyBlock = GetTranslatedExpressionBody(lambda.Body, context);

            var body = bodyBlock.IsASingleStatement
                ? bodyBlock.AsExpressionBody()
                : bodyBlock.WithCurlyBraces();

            return parameters + " =>" + body;
        }
    }
}
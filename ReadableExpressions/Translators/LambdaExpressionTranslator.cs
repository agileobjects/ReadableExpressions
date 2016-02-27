namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = Registry.TranslateParameters(
                lambda.Parameters,
                placeLongListsOnMultipleLines: false,
                encloseSingleParameterInBrackets: false);

            var bodyBlock = Registry.TranslateExpressionBody(lambda.Body);

            var body = bodyBlock.IsASingleStatement
                ? " " + bodyBlock.AsExpressionBody()
                : bodyBlock.WithBrackets();

            return parameters + " =>" + body;
        }
    }
}
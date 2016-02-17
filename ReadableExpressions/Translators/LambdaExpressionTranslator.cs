namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator()
            : base(ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = translatorRegistry.TranslateParameters(
                lambda.Parameters,
                placeLongListsOnMultipleLines: false,
                encloseSingleParameterInBrackets: false);

            var bodyBlock = translatorRegistry.TranslateExpressionBody(
                lambda.Body,
                lambda.ReturnType);

            var body = bodyBlock.IsASingleStatement
                ? " " + bodyBlock.AsExpressionBody()
                : bodyBlock.WithBrackets();

            return parameters + " =>" + body;
        }
    }
}
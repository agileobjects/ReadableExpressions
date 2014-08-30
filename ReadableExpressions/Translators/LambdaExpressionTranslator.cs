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
            var lambdaExpression = (LambdaExpression)expression;

            var parameters = TranslationHelper.GetParameters(
                lambdaExpression.Parameters,
                translatorRegistry,
                encloseSingleParameterInBrackets: false);

            var body = translatorRegistry.Translate(lambdaExpression.Body);

            if ((body[0] == '(') && (body[body.Length - 1] == ')'))
            {
                body = body.Substring(1, body.Length - 2);
            }

            return parameters + " => " + body;
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator()
            : base(ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var lambdaExpression = (LambdaExpression)expression;

            var parameters = GetParameters(lambdaExpression, translatorRegistry);
            var body = translatorRegistry.Translate(lambdaExpression.Body);

            return parameters + " => " + body;
        }

        private static string GetParameters(
            LambdaExpression lambdaExpression,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            if (lambdaExpression.Parameters.Count == 0)
            {
                return "()";
            }

            var parameters = string.Join(
                ", ",
                lambdaExpression.Parameters.Select(translatorRegistry.Translate));

            if (lambdaExpression.Parameters.Count > 1)
            {
                parameters = "(" + parameters + ")";
            }

            return parameters;
        }
    }
}
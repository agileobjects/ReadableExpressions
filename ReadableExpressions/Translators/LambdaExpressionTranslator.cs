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

            var parameters = TranslationHelper.GetParameters(lambdaExpression.Parameters, translatorRegistry);
            var body = translatorRegistry.Translate(lambdaExpression.Body);

            return parameters + " => " + body;
        }
    }
}
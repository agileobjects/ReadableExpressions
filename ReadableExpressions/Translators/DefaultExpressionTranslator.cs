namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class DefaultExpressionTranslator : ExpressionTranslatorBase
    {
        public DefaultExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Default)
        {
        }

        public override string Translate(Expression expression)
        {
            var defaultExpression = (DefaultExpression)expression;

            if (defaultExpression.Type == typeof(void))
            {
                return null;
            }

            var typeName = defaultExpression.Type.GetFriendlyName();

            return $"default({typeName})";
        }
    }
}
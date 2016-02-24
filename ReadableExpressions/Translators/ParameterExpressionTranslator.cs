namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ParameterExpressionTranslator : ExpressionTranslatorBase
    {
        internal ParameterExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Parameter)
        {
        }

        public override string Translate(Expression expression)
        {
            return ((ParameterExpression)expression).Name;
        }
    }
}
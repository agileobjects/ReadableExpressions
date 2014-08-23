namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ParameterExpressionTranslator : ExpressionTranslatorBase
    {
        internal ParameterExpressionTranslator()
            : base(ExpressionType.Parameter)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            return ((ParameterExpression)expression).Name;
        }
    }
}
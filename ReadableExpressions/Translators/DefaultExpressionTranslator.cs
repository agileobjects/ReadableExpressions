namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class DefaultExpressionTranslator : ExpressionTranslatorBase
    {
        public DefaultExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Default)
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
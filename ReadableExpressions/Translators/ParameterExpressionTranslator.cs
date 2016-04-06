namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ParameterExpressionTranslator : ExpressionTranslatorBase
    {
        internal ParameterExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Parameter)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return ((ParameterExpression)expression).Name;
        }
    }
}
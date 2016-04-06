namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.New)
        {
        }

        public override string Translate(Expression expression)
        {
            var newExpression = (NewExpression)expression;
            var parameters = GetTranslatedParameters(newExpression.Arguments).WithBrackets();

            return "new " + newExpression.Type.GetFriendlyName() + parameters;
        }
    }
}
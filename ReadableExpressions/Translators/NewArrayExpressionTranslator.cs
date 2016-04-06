namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewArrayExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewArrayExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.NewArrayBounds)
        {
        }

        public override string Translate(Expression expression)
        {
            var newArrayExpression = (NewArrayExpression)expression;

            var bounds = string.Join(
                string.Empty,
                newArrayExpression.Expressions.Select(exp => "[" + GetTranslation(exp) + "]"));

            return "new " + expression.Type.GetElementType().GetFriendlyName() + bounds;
        }
    }
}
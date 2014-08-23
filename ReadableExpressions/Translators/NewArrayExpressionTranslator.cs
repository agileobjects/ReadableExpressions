namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewArrayExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewArrayExpressionTranslator()
            : base(ExpressionType.NewArrayBounds)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var newArrayExpression = (NewArrayExpression)expression;

            var bounds = string.Join(
                string.Empty,
                newArrayExpression.Expressions.Select(exp => "[" + translatorRegistry.Translate(exp) + "]"));

            return "new " + expression.Type.GetElementType().GetFriendlyName() + bounds;
        }
    }
}
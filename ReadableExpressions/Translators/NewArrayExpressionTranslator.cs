namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewArrayExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewArrayExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.NewArrayBounds)
        {
        }

        public override string Translate(Expression expression)
        {
            var newArrayExpression = (NewArrayExpression)expression;

            var bounds = string.Join(
                string.Empty,
                newArrayExpression.Expressions.Select(exp => "[" + Registry.Translate(exp) + "]"));

            return "new " + expression.Type.GetElementType().GetFriendlyName() + bounds;
        }
    }
}
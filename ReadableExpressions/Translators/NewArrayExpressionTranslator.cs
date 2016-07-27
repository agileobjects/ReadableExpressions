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

        public override string Translate(Expression expression, TranslationContext context)
        {
            var newArray = (NewArrayExpression)expression;

            var arrayTypeName = expression.Type.GetElementType().GetFriendlyName();

            var bounds = string.Join(
                "][",
                newArray.Expressions.Select(context.Translate));

            return $"new {arrayTypeName}[{bounds}]";
        }
    }
}
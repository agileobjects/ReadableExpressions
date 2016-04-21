namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewArrayExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewArrayExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.NewArrayBounds)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var newArray = (NewArrayExpression)expression;

            var arrayTypeName = expression.Type.GetElementType().GetFriendlyName();

            var bounds = string.Join(
                "][",
                newArray.Expressions.Select(exp => GetTranslation(exp, context)));

            return $"new {arrayTypeName}[{bounds}]";
        }
    }
}
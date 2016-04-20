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
            var newArrayExpression = (NewArrayExpression)expression;

            var bounds = string.Join(
                string.Empty,
                newArrayExpression.Expressions.Select(exp => "[" + GetTranslation(exp, context) + "]"));

            return "new " + expression.Type.GetElementType().GetFriendlyName() + bounds;
        }
    }
}
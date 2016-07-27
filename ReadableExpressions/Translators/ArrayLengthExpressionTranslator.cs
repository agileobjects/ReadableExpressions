namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator()
            : base(ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var arrayAccess = context.Translate(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}
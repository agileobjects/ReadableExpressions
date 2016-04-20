namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var arrayAccess = GetTranslation(((UnaryExpression)expression).Operand, context);

            return arrayAccess + ".Length";
        }
    }
}
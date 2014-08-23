namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator()
            : base(ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var arrayAccess = translatorRegistry.Translate(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}
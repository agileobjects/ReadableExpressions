namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression)
        {
            var arrayAccess = Registry.Translate(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression)
        {
            var arrayAccess = GetTranslation(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}
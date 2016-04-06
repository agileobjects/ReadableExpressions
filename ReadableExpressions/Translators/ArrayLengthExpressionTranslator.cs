namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
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
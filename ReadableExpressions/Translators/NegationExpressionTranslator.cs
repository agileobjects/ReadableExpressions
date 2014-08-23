namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class NegationExpressionTranslator : ExpressionTranslatorBase
    {
        internal NegationExpressionTranslator()
            : base(ExpressionType.Not)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var negation = (UnaryExpression)expression;

            return "!" + translatorRegistry.Translate(negation.Operand);
        }
    }
}
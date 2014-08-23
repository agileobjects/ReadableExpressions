namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class BinaryExpressionTranslator : ExpressionTranslatorBase
    {
        internal BinaryExpressionTranslator()
            : base(ExpressionType.Equal)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var binaryExpression = (BinaryExpression)expression;
            var left = translatorRegistry.Translate(binaryExpression.Left);
            var right = translatorRegistry.Translate(binaryExpression.Right);

            return left + " == " + right;
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ArrayIndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public ArrayIndexAccessExpressionTranslator()
            : base(ExpressionType.ArrayIndex)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var arrayAccess = (BinaryExpression)expression;
            var arrayVariable = translatorRegistry.Translate(arrayAccess.Left);
            var index = ((ConstantExpression)arrayAccess.Right).Value;

            return $"{arrayVariable}[{index}]";
        }
    }
}
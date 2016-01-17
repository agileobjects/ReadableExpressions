namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class IndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public IndexAccessExpressionTranslator()
            : base(ExpressionType.ArrayIndex)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var arrayAccess = (BinaryExpression)expression;

            return TranslateIndexAccess(arrayAccess.Left, arrayAccess.Right, translatorRegistry);
        }

        internal static string TranslateIndexAccess(
            Expression variable,
            Expression index,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var indexedVariable = translatorRegistry.Translate(variable);
            var indexValue = translatorRegistry.Translate(index);

            return $"{indexedVariable}[{indexValue}]";
        }
    }
}
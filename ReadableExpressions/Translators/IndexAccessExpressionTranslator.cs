namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class IndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public IndexAccessExpressionTranslator()
            : base(ExpressionType.ArrayIndex, ExpressionType.Index)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            if (expression.NodeType == ExpressionType.Index)
            {
                return TranslateIndexedPropertyAccess(expression, translatorRegistry);
            }

            var arrayAccess = (BinaryExpression)expression;

            return TranslateIndexAccess(arrayAccess.Left, new[] { arrayAccess.Right }, translatorRegistry);
        }

        private static string TranslateIndexedPropertyAccess(
            Expression expression,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var index = (IndexExpression)expression;

            return TranslateIndexAccess(index.Object, index.Arguments, translatorRegistry);
        }

        internal static string TranslateIndexAccess(
            Expression variable,
            IEnumerable<Expression> indexes,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var indexedVariable = translatorRegistry.Translate(variable);

            var indexValues = TranslationHelper.GetParameters(
                indexes,
                translatorRegistry,
                placeLongListsOnMultipleLines: false,
                encloseSingleParameterInBrackets: false);

            return $"{indexedVariable}[{indexValues}]";
        }
    }
}
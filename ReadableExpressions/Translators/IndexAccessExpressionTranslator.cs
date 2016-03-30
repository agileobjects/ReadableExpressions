namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Formatting;

    internal class IndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public IndexAccessExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.ArrayIndex, ExpressionType.Index)
        {
        }

        public override string Translate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Index)
            {
                return TranslateIndexedPropertyAccess(expression);
            }

            var arrayAccess = (BinaryExpression)expression;

            return TranslateIndexAccess(arrayAccess.Left, new[] { arrayAccess.Right });
        }

        private string TranslateIndexedPropertyAccess(Expression expression)
        {
            var index = (IndexExpression)expression;

            return TranslateIndexAccess(index.Object, index.Arguments);
        }

        internal string TranslateIndexAccess(Expression variable, IEnumerable<Expression> indexes)
        {
            var indexedVariable = Registry.Translate(variable);

            var indexValues = Registry
                .TranslateParameters(indexes)
                .WithoutBrackets();

            return $"{indexedVariable}[{indexValues}]";
        }
    }
}
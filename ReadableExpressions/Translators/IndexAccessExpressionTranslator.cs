namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class IndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public IndexAccessExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.ArrayIndex, ExpressionType.Index)
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
            var indexedVariable = GetTranslation(variable);
            var indexValues = GetTranslatedParameters(indexes).WithoutBrackets();

            return $"{indexedVariable}[{indexValues}]";
        }
    }
}
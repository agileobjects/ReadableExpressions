namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class IndexAccessExpressionTranslator : ExpressionTranslatorBase
    {
        public IndexAccessExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.ArrayIndex, ExpressionType.Index)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            if (expression.NodeType == ExpressionType.Index)
            {
                return TranslateIndexedPropertyAccess(expression, context);
            }

            var arrayAccess = (BinaryExpression)expression;

            return TranslateIndexAccess(arrayAccess.Left, new[] { arrayAccess.Right }, context);
        }

        private string TranslateIndexedPropertyAccess(Expression expression, TranslationContext context)
        {
            var index = (IndexExpression)expression;

            return TranslateIndexAccess(index.Object, index.Arguments, context);
        }

        internal string TranslateIndexAccess(
            Expression variable,
            IEnumerable<Expression> indexes,
            TranslationContext context)
        {
            var indexedVariable = GetTranslation(variable, context);
            var indexValues = GetTranslatedParameters(indexes, context).WithoutParentheses();

            return $"{indexedVariable}[{indexValues}]";
        }
    }
}
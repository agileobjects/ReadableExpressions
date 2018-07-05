namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using BinaryExpression = Microsoft.Scripting.Ast.BinaryExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using IndexExpression = Microsoft.Scripting.Ast.IndexExpression;
#endif

    internal struct IndexAccessExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get
            {
                yield return ExpressionType.ArrayIndex;
                yield return ExpressionType.Index;
            }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            if (expression.NodeType == ExpressionType.Index)
            {
                return TranslateIndexedPropertyAccess(expression, context);
            }

            var arrayAccess = (BinaryExpression)expression;

            return TranslateIndexAccess(arrayAccess.Left, new[] { arrayAccess.Right }, context);
        }

        private static string TranslateIndexedPropertyAccess(Expression expression, TranslationContext context)
        {
            var index = (IndexExpression)expression;

            return TranslateIndexAccess(index.Object, index.Arguments, context);
        }

        internal static string TranslateIndexAccess(
            Expression variable,
            IEnumerable<Expression> indexes,
            TranslationContext context)
        {
            var indexedVariable = context.Translate(variable);
            var indexValues = context.TranslateParameters(indexes).WithoutParentheses();

            return $"{indexedVariable}[{indexValues}]";
        }
    }
}
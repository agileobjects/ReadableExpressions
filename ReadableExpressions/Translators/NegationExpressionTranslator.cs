namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using Extensions;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif

    internal struct NegationExpressionTranslator : IExpressionTranslator
    {
        private static readonly Dictionary<ExpressionType, string> _negationsByNodeType = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Not] = "!",
            [ExpressionType.Negate] = "-",
            [ExpressionType.NegateChecked] = "-"
        };

        public IEnumerable<ExpressionType> NodeTypes => _negationsByNodeType.Keys;

        public string Translate(Expression expression, TranslationContext context)
        {
            var negation = (UnaryExpression)expression;

            return Translate(expression.NodeType, negation.Operand, context);
        }

        public static string TranslateNot(Expression expression, TranslationContext context) 
            => Translate(ExpressionType.Not, expression, context);

        private static string Translate(ExpressionType negationType, Expression expression, TranslationContext context)
        {
            var valueToNegate = context.Translate(expression);

            if (WrapNegatedValue(valueToNegate, expression))
            {
                valueToNegate = valueToNegate.WithSurroundingParentheses();
            }

            return _negationsByNodeType[negationType] + valueToNegate;
        }

        private static bool WrapNegatedValue(string value, Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                return false;
            }

            return value.Contains(" ");
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class NegationExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, string> _negationsByNodeType = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Not] = "!",
            [ExpressionType.Negate] = "-",
            [ExpressionType.NegateChecked] = "-"
        };

        internal NegationExpressionTranslator()
            : base(_negationsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var negation = (UnaryExpression)expression;

            return Translate(expression.NodeType, negation.Operand, context);
        }

        public string TranslateNot(Expression expression, TranslationContext context)
        {
            return Translate(ExpressionType.Not, expression, context);
        }

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
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class BinaryExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, string> _operatorsByNodeType =
            new Dictionary<ExpressionType, string>
            {
                { ExpressionType.Add, "+" },
                { ExpressionType.And, "&" },
                { ExpressionType.AndAlso, "&&" },
                { ExpressionType.Coalesce, "??" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.Equal, "==" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.NotEqual, "!=" },
                { ExpressionType.Or, "|" },
                { ExpressionType.OrElse, "||" },
                { ExpressionType.Subtract, "-" },
            };

        internal BinaryExpressionTranslator()
            : base(_operatorsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var binaryExpression = (BinaryExpression)expression;
            var left = translatorRegistry.Translate(binaryExpression.Left);
            var @operator = _operatorsByNodeType[expression.NodeType];
            var right = translatorRegistry.Translate(binaryExpression.Right);

            return "(" + left + " " + @operator + " " + right + ")";
        }
    }
}
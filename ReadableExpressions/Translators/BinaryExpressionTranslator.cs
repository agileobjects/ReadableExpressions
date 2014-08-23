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
                { ExpressionType.Equal, "==" },
                { ExpressionType.NotEqual, "!=" },
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

            return left + " " + @operator + " " + right;
        }
    }
}
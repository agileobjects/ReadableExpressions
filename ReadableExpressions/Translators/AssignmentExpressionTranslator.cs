namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class AssignmentExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, string> _symbolsByNodeType =
            new Dictionary<ExpressionType, string>
            {
                { ExpressionType.AddAssign, "+=" },
                { ExpressionType.Assign, "=" }
            };

        internal AssignmentExpressionTranslator()
            : base(_symbolsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var assignment = (BinaryExpression)expression;
            var target = translatorRegistry.Translate(assignment.Left);
            var symbol = _symbolsByNodeType[expression.NodeType];
            var value = translatorRegistry.Translate(assignment.Right);

            return $"{target} {symbol} {value}";
        }
    }
}
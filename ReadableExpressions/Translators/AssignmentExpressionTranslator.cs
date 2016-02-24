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
                [ExpressionType.AddAssign] = "+=",
                [ExpressionType.AndAssign] = "&=",
                [ExpressionType.Assign] = "=",
                [ExpressionType.DivideAssign] = "/=",
                [ExpressionType.ExclusiveOrAssign] = "^=",
                [ExpressionType.LeftShiftAssign] = "<<=",
                [ExpressionType.ModuloAssign] = @"%=",
                [ExpressionType.MultiplyAssign] = "*=",
                [ExpressionType.OrAssign] = "|=",
                [ExpressionType.PowerAssign] = "**=",
                [ExpressionType.RightShiftAssign] = ">>=",
                [ExpressionType.SubtractAssign] = "-="
            };

        internal AssignmentExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, _symbolsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression)
        {
            var assignment = (BinaryExpression)expression;
            var target = Registry.Translate(assignment.Left);
            var symbol = _symbolsByNodeType[expression.NodeType];
            var value = Registry.Translate(assignment.Right);

            return $"{target} {symbol} {value}";
        }
    }
}
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
                [ExpressionType.AddAssignChecked] = "+=",
                [ExpressionType.AndAssign] = "&=",
                [ExpressionType.Assign] = "=",
                [ExpressionType.DivideAssign] = "/=",
                [ExpressionType.ExclusiveOrAssign] = "^=",
                [ExpressionType.LeftShiftAssign] = "<<=",
                [ExpressionType.ModuloAssign] = @"%=",
                [ExpressionType.MultiplyAssign] = "*=",
                [ExpressionType.MultiplyAssignChecked] = "*=",
                [ExpressionType.OrAssign] = "|=",
                [ExpressionType.PowerAssign] = "**=",
                [ExpressionType.RightShiftAssign] = ">>=",
                [ExpressionType.SubtractAssign] = "-=",
                [ExpressionType.SubtractAssignChecked] = "-="
            };

        internal AssignmentExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, _symbolsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression)
        {
            var assignment = (BinaryExpression)expression;
            var target = Registry.Translate(assignment.Left);

            return GetAssignment(target, expression.NodeType, assignment.Right);
        }

        internal string GetAssignment(
            string target,
            ExpressionType assignmentType,
            Expression right)
        {
            var symbol = _symbolsByNodeType[assignmentType];
            var value = Registry.Translate(right);

            return $"{target} {symbol} {value}";
        }
    }
}
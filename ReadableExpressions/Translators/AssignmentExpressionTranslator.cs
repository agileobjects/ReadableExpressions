namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using BinaryExpression = Microsoft.Scripting.Ast.BinaryExpression;
    using DefaultExpression = Microsoft.Scripting.Ast.DefaultExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
    using Extensions;

    internal struct AssignmentExpressionTranslator : IExpressionTranslator
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

        private static readonly ExpressionType[] _checkedAssignmentTypes =
            _symbolsByNodeType.GetCheckedExpressionTypes();

        public IEnumerable<ExpressionType> NodeTypes => _symbolsByNodeType.Keys;

        public string Translate(Expression expression, TranslationContext context)
        {
            var assignment = (BinaryExpression)expression;
            var target = context.Translate(assignment.Left);

            return GetAssignment(target, expression.NodeType, assignment.Right, context);
        }

        internal static string GetAssignment(
            string target,
            ExpressionType assignmentType,
            Expression value,
            TranslationContext context)
        {
            var symbol = _symbolsByNodeType[assignmentType];

            var valueString = (value.NodeType == ExpressionType.Default)
                ? DefaultExpressionTranslator.Translate((DefaultExpression)value, context.Settings)
                : GetValueTranslation(value, context);

            var assignment = target + " " + symbol;

            if (!valueString.StartsWithNewLine())
            {
                assignment += " ";
            }

            assignment += valueString;

            assignment = AdjustForCheckedAssignmentIfAppropriate(assignmentType, assignment);

            return assignment;
        }

        private static string GetValueTranslation(Expression value, TranslationContext context)
        {
            var valueBlock = context.TranslateCodeBlock(value);

            if (valueBlock.IsASingleStatement)
            {
                return valueBlock.WithoutCurlyBraces().Unterminated();
            }

            if ((value.NodeType == ExpressionType.Conditional) ||
                (value.NodeType == ExpressionType.Lambda))
            {
                return valueBlock.WithoutCurlyBraces();
            }

            return valueBlock.WithCurlyBraces();
        }

        private static string AdjustForCheckedAssignmentIfAppropriate(
            ExpressionType assignmentType,
            string assignment)
        {
            if (Array.IndexOf(_checkedAssignmentTypes, assignmentType) == -1)
            {
                return assignment;
            }

            if (assignment.IsMultiLine())
            {
                return $@"
checked
{{
{assignment.Indented()}
}}".TrimStart();
            }

            return $"checked {{ {assignment} }}";
        }
    }
}
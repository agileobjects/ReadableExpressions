namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

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

        private static readonly ExpressionType[] _checkedAssignmentTypes =
            _symbolsByNodeType.GetCheckedExpressionTypes();

        private readonly DefaultExpressionTranslator _defaultTranslator;

        internal AssignmentExpressionTranslator(DefaultExpressionTranslator defaultTranslator)
            : base(_symbolsByNodeType.Keys.ToArray())
        {
            _defaultTranslator = defaultTranslator;
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var assignment = (BinaryExpression)expression;
            var target = context.GetTranslation(assignment.Left);

            return GetAssignment(target, expression.NodeType, assignment.Right, context);
        }

        internal string GetAssignment(
            string target,
            ExpressionType assignmentType,
            Expression value,
            TranslationContext context)
        {
            var symbol = _symbolsByNodeType[assignmentType];

            var valueString = (value.NodeType == ExpressionType.Default)
                ? _defaultTranslator.Translate((DefaultExpression)value)
                : GetValueTranslation(value, context);

            var assignment = $"{target} {symbol} {valueString}";

            assignment = AdjustForCheckedAssignmentIfAppropriate(assignmentType, assignment);

            return assignment;
        }

        private string GetValueTranslation(Expression value, TranslationContext context)
        {
            var valueBlock = GetTranslatedExpressionBody(value, context);

            if (valueBlock.IsASingleStatement)
            {
                return valueBlock.WithoutParentheses().Unterminated();
            }

            if (value.NodeType == ExpressionType.Lambda)
            {
                return valueBlock.WithoutParentheses();
            }

            return valueBlock.WithReturn().WithParentheses();
        }

        private static string AdjustForCheckedAssignmentIfAppropriate(
            ExpressionType assignmentType,
            string assignment)
        {
            if (!_checkedAssignmentTypes.Contains(assignmentType))
            {
                return assignment;
            }

            if (assignment.IsMultiLine())
            {
                return $@"
checked
{{
{assignment.Indent()}
}}".TrimStart();
            }

            return $"checked {{ {assignment} }}";
        }
    }
}
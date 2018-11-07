namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class AssignmentTranslation :
        CheckedOperationTranslationBase,
        ITranslation,
        IPotentialSelfTerminatingTranslatable
    {
        private static readonly Dictionary<ExpressionType, string> _symbolsByNodeType =
            new Dictionary<ExpressionType, string>
            {
                [AddAssign] = " +=",
                [AddAssignChecked] = " +=",
                [AndAssign] = " &=",
                [Assign] = " =",
                [DivideAssign] = " /=",
                [ExclusiveOrAssign] = " ^=",
                [LeftShiftAssign] = " <<=",
                [ModuloAssign] = " %=",
                [MultiplyAssign] = " *=",
                [MultiplyAssignChecked] = " *=",
                [OrAssign] = " |=",
                [PowerAssign] = " **=",
                [RightShiftAssign] = " >>=",
                [SubtractAssign] = " -=",
                [SubtractAssignChecked] = " -="
            };

        private readonly ITranslation _targetTranslation;
        private readonly string _operator;
        private readonly ITranslation _valueTranslation;

        public AssignmentTranslation(BinaryExpression assignment, ITranslationContext context)
            : base(IsCheckedAssignment(assignment.NodeType), " { ", " }")
        {
            NodeType = assignment.NodeType;
            _targetTranslation = context.GetCodeBlockTranslationFor(assignment.Left);
            _operator = _symbolsByNodeType[NodeType];
            _valueTranslation = GetValueTranslation(assignment.Right, context);
            EstimatedSize = GetEstimatedSize();
        }

        private static bool IsCheckedAssignment(ExpressionType assignmentType)
        {
            switch (assignmentType)
            {
                case AddAssignChecked:
                case MultiplyAssignChecked:
                case SubtractAssignChecked:
                    return true;
            }

            return false;
        }

        private ITranslation GetValueTranslation(Expression assignedValue, ITranslationContext context)
        {
            return (assignedValue.NodeType == Default)
                ? new DefaultValueTranslation(assignedValue, context, allowNullKeyword: assignedValue.Type == typeof(string))
                : GetNonDefaultValueTranslation(assignedValue, context);
        }

        private ITranslation GetNonDefaultValueTranslation(Expression assignedValue, ITranslationContext context)
        {
            var valueBlock = context.GetCodeBlockTranslationFor(assignedValue);

            if (valueBlock.IsMultiStatement())
            {
                return (valueBlock.NodeType == Conditional) || (valueBlock.NodeType == Lambda)
                    ? valueBlock.WithoutBraces()
                    : valueBlock.WithBraces();
            }

            return IsCheckedOperation
                ? valueBlock.WithoutBraces().WithTermination()
                : valueBlock.WithoutBraces().WithoutTermination();
        }

        private int GetEstimatedSize()
        {
            var estimatedSize =
               _targetTranslation.EstimatedSize +
               _operator.Length +
               _valueTranslation.EstimatedSize;

            if (IsCheckedOperation)
            {
                estimatedSize += 10;
            }

            return estimatedSize;
        }

        public static bool IsAssignment(ExpressionType nodeType) => _symbolsByNodeType.ContainsKey(nodeType);

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public bool IsTerminated => _valueTranslation.IsTerminated();

        protected override bool IsMultiStatement()
            => _targetTranslation.IsMultiStatement() || _valueTranslation.IsMultiStatement();

        public void WriteTo(ITranslationContext context)
        {
            WriteOpeningCheckedIfNecessary(context, out var isMultiStatementChecked);
            _targetTranslation.WriteTo(context);
            context.WriteToTranslation(_operator);

            if (_valueTranslation.IsMultiStatement() == false)
            {
                context.WriteSpaceToTranslation();
            }

            _valueTranslation.WriteTo(context);

            WriteClosingCheckedIfNecessary(context, isMultiStatementChecked);
        }
    }
}
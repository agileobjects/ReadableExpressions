namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
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
        private bool _suppressSpaceBeforeValue;

        public AssignmentTranslation(BinaryExpression assignment, ITranslationContext context)
            : this(
                assignment.NodeType,
                context.GetCodeBlockTranslationFor(assignment.Left),
                assignment.Right,
                context)
        {
        }

        public AssignmentTranslation(
            ExpressionType nodeType,
            ITranslation targetTranslation,
            Expression value,
            ITranslationContext context)
            : base(IsCheckedAssignment(nodeType), " { ", " }")
        {
            NodeType = nodeType;
            _targetTranslation = targetTranslation;
            _operator = _symbolsByNodeType[NodeType];
            _valueTranslation = GetValueTranslation(value, context);
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

            if (valueBlock.IsMultiStatement == false)
            {
                return IsCheckedOperation
                    ? valueBlock.WithoutBraces().WithTermination()
                    : valueBlock.WithoutBraces().WithoutTermination();
            }

            if (valueBlock.IsMultiStatementLambda(context))
            {
                return valueBlock.WithoutBraces();
            }

            _suppressSpaceBeforeValue = true;

            return (valueBlock.NodeType == Conditional)
                ? valueBlock.WithoutBraces() 
                : valueBlock.WithBraces();
        }

        private int GetEstimatedSize()
        {
            var estimatedSize =
               _targetTranslation.EstimatedSize +
               _operator.Length + 1 +
               _valueTranslation.EstimatedSize;

            if (IsCheckedOperation)
            {
                estimatedSize += 10;
            }

            return estimatedSize;
        }

        public static bool IsAssignment(ExpressionType nodeType) => _symbolsByNodeType.ContainsKey(nodeType);

        public ExpressionType NodeType { get; }

        public Type Type => _targetTranslation.Type;

        public int EstimatedSize { get; }

        public bool IsTerminated => _valueTranslation.IsTerminated();

        protected override bool IsMultiStatement()
            => _targetTranslation.IsMultiStatement() || _valueTranslation.IsMultiStatement();

        public void WriteTo(TranslationBuffer buffer)
        {
            WriteOpeningCheckedIfNecessary(buffer, out var isMultiStatementChecked);
            _targetTranslation.WriteTo(buffer);
            buffer.WriteToTranslation(_operator);

            if (_suppressSpaceBeforeValue == false)
            {
                buffer.WriteSpaceToTranslation();
            }

            _valueTranslation.WriteTo(buffer);

            WriteClosingCheckedIfNecessary(buffer, isMultiStatementChecked);
        }
    }
}
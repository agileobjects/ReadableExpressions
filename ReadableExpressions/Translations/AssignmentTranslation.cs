namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class AssignmentTranslation :
        CheckedOperationTranslationBase,
        ITranslation,
        IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslation _targetTranslation;
        private readonly TranslationSettings _settings;
        private readonly string _operator;
        private readonly ITranslation _valueTranslation;
        private bool _suppressSpaceBeforeValue;

        public AssignmentTranslation(BinaryExpression assignment, ITranslationContext context)
            : this(
                assignment.NodeType,
                GetTargetTranslation(assignment.Left, context),
                assignment.Right,
                context)
        {
        }

        private static ITranslation GetTargetTranslation(Expression target, ITranslationContext context)
        {
            return (target.NodeType == Parameter)
                ? context.GetTranslationFor(target)
                : context.GetCodeBlockTranslationFor(target);
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
            _settings = context.Settings;
            _operator = GetOperatorOrNull(nodeType);
            _valueTranslation = GetValueTranslation(value, context);
            TranslationSize = GetTranslationSize();
            FormattingSize = _targetTranslation.FormattingSize + _valueTranslation.FormattingSize;
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

        private static string GetOperatorOrNull(ExpressionType assignmentType)
        {
            return assignmentType switch
            {
                AddAssign => " +=",
                AddAssignChecked => " +=",
                AndAssign => " &=",
                Assign => " =",
                DivideAssign => " /=",
                ExclusiveOrAssign => " ^=",
                LeftShiftAssign => " <<=",
                ModuloAssign => " %=",
                MultiplyAssign => " *=",
                MultiplyAssignChecked => " *=",
                OrAssign => " |=",
                PowerAssign => " **=",
                RightShiftAssign => " >>=",
                SubtractAssign => " -=",
                SubtractAssignChecked => " -=",
                _ => null
            };
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

            if (valueBlock.IsMultiStatementLambda)
            {
                return valueBlock.WithoutBraces();
            }

            _suppressSpaceBeforeValue = true;

            return (valueBlock.NodeType == Conditional)
                ? valueBlock.WithoutBraces()
                : valueBlock.WithBraces();
        }

        private int GetTranslationSize()
        {
            var translationSize =
               _targetTranslation.TranslationSize +
               _operator.Length + 1 +
               _valueTranslation.TranslationSize;

            if (IsCheckedOperation)
            {
                translationSize += 10;
            }

            return translationSize;
        }

        public static bool IsAssignment(ExpressionType nodeType) => GetOperatorOrNull(nodeType) != null;

        public ExpressionType NodeType { get; }

        public Type Type => _targetTranslation.Type;

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsTerminated => _valueTranslation.IsTerminated();

        protected override bool IsMultiStatement()
            => _targetTranslation.IsMultiStatement() || _valueTranslation.IsMultiStatement();

        public int GetIndentSize()
        {
            var indentSize =
                _targetTranslation.GetIndentSize() +
                _valueTranslation.GetIndentSize();

            if (IsCheckedOperation && IsMultiStatement())
            {
                indentSize +=
                    _targetTranslation.GetLineCount() * _settings.IndentLength +
                    _valueTranslation.GetLineCount() * _settings.IndentLength;
            }

            return indentSize;
        }

        public int GetLineCount()
        {
            var targetLineCount = _targetTranslation.GetLineCount();
            var valueLineCount = _valueTranslation.GetLineCount();

            var lineCount = (targetLineCount == 1)
                ? (valueLineCount == 1) ? 1 : targetLineCount
                : (valueLineCount == 1) ? 1 : valueLineCount;

            if (IsCheckedOperation && IsMultiStatement())
            {
                lineCount += 2;
            }

            return lineCount;
        }

        public void WriteTo(TranslationWriter writer)
        {
            WriteOpeningCheckedIfNecessary(writer, out var isMultiStatementChecked);
            _targetTranslation.WriteTo(writer);
            writer.WriteToTranslation(_operator);

            if (_suppressSpaceBeforeValue == false)
            {
                writer.WriteSpaceToTranslation();
            }

            _valueTranslation.WriteTo(writer);

            WriteClosingCheckedIfNecessary(writer, isMultiStatementChecked);
        }
    }
}
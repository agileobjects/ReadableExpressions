namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class BinaryTranslation : ITranslation
    {
        private static readonly Dictionary<ExpressionType, string> _operatorsByNodeType =
            new Dictionary<ExpressionType, string>(23)
            {
                [Add] = " + ",
                [AddChecked] = " + ",
                [And] = " & ",
                [AndAlso] = " && ",
                [Coalesce] = " ?? ",
                [Divide] = " / ",
                [Equal] = " == ",
                [ExclusiveOr] = " ^ ",
                [GreaterThan] = " > ",
                [GreaterThanOrEqual] = " >= ",
                [LeftShift] = " << ",
                [LessThan] = " < ",
                [LessThanOrEqual] = " <= ",
                [Modulo] = " % ",
                [Multiply] = " * ",
                [MultiplyChecked] = " * ",
                [NotEqual] = " != ",
                [Or] = " | ",
                [OrElse] = " || ",
                [Power] = " ** ",
                [RightShift] = " >> ",
                [Subtract] = " - ",
                [SubtractChecked] = " - "
            };

        private readonly ITranslation _leftOperandTranslation;
        private readonly string _operator;
        private readonly ITranslation _rightOperandTranslation;
        private readonly StandaloneBoolean _standaloneBoolean;
        private readonly bool _isCheckedOperation;
        private readonly Action<ITranslationContext> _translationWriter;

        public BinaryTranslation(BinaryExpression binary, ITranslationContext context)
        {
            NodeType = binary.NodeType;

            switch (NodeType)
            {
                case Add:
                    if (binary.Type != typeof(string))
                    {
                        goto default;
                    }

                    var operands = new[] { binary.Left, binary.Right };
                    _translationWriter = new StringConcatenationTranslation(operands, context).WriteTo;
                    break;

                case Equal:
                case NotEqual:
                    if (TryGetStandaloneBoolean(binary, out _standaloneBoolean))
                    {
                        _leftOperandTranslation = context.GetTranslationFor(_standaloneBoolean.Expression);
                        EstimatedSize = _leftOperandTranslation.EstimatedSize + 1;
                        _translationWriter = WriteStandaloneEqualityComparison;
                        break;
                    }

                    goto default;

                default:
                    _leftOperandTranslation = context.GetTranslationFor(binary.Left);
                    _operator = GetOperator(binary);
                    _rightOperandTranslation = context.GetTranslationFor(binary.Right);
                    _isCheckedOperation = IsCheckedOperation();
                    EstimatedSize = GetEstimatedSize();
                    break;
            }
        }

        public static string GetOperator(BinaryExpression expression) => _operatorsByNodeType[expression.NodeType];

        public static bool IsBinary(ExpressionType nodeType) => _operatorsByNodeType.ContainsKey(nodeType);

        private bool IsCheckedOperation()
        {
            switch (NodeType)
            {
                case AddChecked:
                case MultiplyChecked:
                case SubtractChecked:
                    return true;
            }

            return false;
        }

        private int GetEstimatedSize()
        {
            var estimatedSize =
                _leftOperandTranslation.EstimatedSize +
               _operator.Length +
               _rightOperandTranslation.EstimatedSize;

            if (_isCheckedOperation)
            {
                estimatedSize += 10;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        private void WriteStandaloneEqualityComparison(ITranslationContext context)
        {
            if (_standaloneBoolean.IsComparisonToTrue)
            {
                _leftOperandTranslation.WriteTo(context);
                return;
            }

            NegationTranslation.ForNot(_leftOperandTranslation).WriteTo(context);
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_translationWriter != null)
            {
                _translationWriter.Invoke(context);
                return;
            }

            WriteOpeningCheckedIfNecessary(context, out var isMultiStatementChecked);
            _leftOperandTranslation.WriteInParenthesesIfRequired(context);
            context.WriteToTranslation(_operator);
            _rightOperandTranslation.WriteInParenthesesIfRequired(context);
            WriteClosingCheckedIfNecessary(context, isMultiStatementChecked);
        }

        private void WriteOpeningCheckedIfNecessary(ITranslationContext context, out bool isMultiStatementChecked)
        {
            if (_isCheckedOperation == false)
            {
                isMultiStatementChecked = false;
                return;
            }

            context.WriteToTranslation("checked");

            isMultiStatementChecked =
                _leftOperandTranslation.IsMultiStatement() ||
                _rightOperandTranslation.IsMultiStatement();

            if (isMultiStatementChecked)
            {
                context.WriteOpeningBraceToTranslation();
                return;
            }

            context.WriteToTranslation('(');
        }

        private void WriteClosingCheckedIfNecessary(ITranslationContext context, bool isMultiStatementChecked)
        {
            if (_isCheckedOperation == false)
            {
                return;
            }

            if (isMultiStatementChecked)
            {
                context.WriteClosingBraceToTranslation();
                return;
            }

            context.WriteToTranslation(')');
        }

        private static bool TryGetStandaloneBoolean(BinaryExpression comparison, out StandaloneBoolean standalone)
        {
            if (IsBooleanConstant(comparison.Right))
            {
                standalone = new StandaloneBoolean(comparison.Left, comparison.NodeType, comparison.Right);
                return true;
            }

            if (IsBooleanConstant(comparison.Left))
            {
                standalone = new StandaloneBoolean(comparison.Right, comparison.NodeType, comparison.Left);
                return true;
            }

            standalone = null;
            return false;
        }

        private static bool IsBooleanConstant(Expression expression)
        {
            return ((expression.NodeType == Constant) || (expression.NodeType == Default)) &&
                    (expression.Type == typeof(bool));
        }

        private class StandaloneBoolean
        {
            public StandaloneBoolean(
                Expression boolean,
                ExpressionType @operator,
                Expression comparison)
            {
                Expression = boolean;

                var comparisonValue =
                    (comparison.NodeType != Default) &&
                    (bool)((ConstantExpression)comparison).Value;

                IsComparisonToTrue =
                    (comparisonValue && (@operator == Equal)) ||
                    (!comparisonValue && (@operator == NotEqual));
            }

            public Expression Expression { get; }

            public bool IsComparisonToTrue { get; }
        }
    }
}
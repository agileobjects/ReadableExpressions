﻿namespace AgileObjects.ReadableExpressions.Translations
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
    using Interfaces;

    internal class BinaryTranslation : CheckedOperationTranslationBase, ITranslation
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

        private BinaryTranslation(BinaryExpression binary, ITranslationContext context)
            : base(IsCheckedBinary(binary.NodeType), "(", ")")
        {
            NodeType = binary.NodeType;
            Type = binary.Type;
            _leftOperandTranslation = context.GetTranslationFor(binary.Left);
            _operator = GetOperator(binary);
            _rightOperandTranslation = context.GetTranslationFor(binary.Right);
            EstimatedSize = GetEstimatedSize();
        }

        public static ITranslation For(BinaryExpression binary, ITranslationContext context)
        {
            switch (binary.NodeType)
            {
                case Add:
                    if (binary.Type != typeof(string))
                    {
                        break;
                    }

                    var operands = new[] { binary.Left, binary.Right };
                    return new StringConcatenationTranslation(Add, operands, context);

                case Equal:
                case NotEqual:
                    if (StandaloneEqualityComparisonTranslation.TryGetTranslation(binary, context, out var translation))
                    {
                        return translation;
                    }

                    break;
            }

            return new BinaryTranslation(binary, context);
        }

        private static bool IsCheckedBinary(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case AddChecked:
                case MultiplyChecked:
                case SubtractChecked:
                    return true;
            }

            return false;
        }

        public static string GetOperator(BinaryExpression expression) => _operatorsByNodeType[expression.NodeType];

        public static bool IsBinary(ExpressionType nodeType) => _operatorsByNodeType.ContainsKey(nodeType);

        private int GetEstimatedSize()
        {
            var estimatedSize =
                _leftOperandTranslation.EstimatedSize +
               _operator.Length +
               _rightOperandTranslation.EstimatedSize;

            if (IsCheckedOperation)
            {
                estimatedSize += 10;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            WriteOpeningCheckedIfNecessary(buffer, out var isMultiStatementChecked);
            _leftOperandTranslation.WriteInParenthesesIfRequired(buffer);
            buffer.WriteToTranslation(_operator);
            _rightOperandTranslation.WriteInParenthesesIfRequired(buffer);
            WriteClosingCheckedIfNecessary(buffer, isMultiStatementChecked);
        }

        protected override bool IsMultiStatement()
            => _leftOperandTranslation.IsMultiStatement() || _rightOperandTranslation.IsMultiStatement();

        private class StandaloneEqualityComparisonTranslation : ITranslation
        {
            private readonly StandaloneBoolean _standaloneBoolean;
            private readonly ITranslation _operandTranslation;

            private StandaloneEqualityComparisonTranslation(
                ExpressionType nodeType,
                Expression boolean,
                ExpressionType @operator,
                Expression comparison,
                ITranslationContext context)
            {
                NodeType = nodeType;
                _standaloneBoolean = new StandaloneBoolean(boolean, @operator, comparison);
                _operandTranslation = context.GetTranslationFor(_standaloneBoolean.Expression);
                EstimatedSize = _operandTranslation.EstimatedSize + 1;
            }

            public static bool TryGetTranslation(BinaryExpression comparison, ITranslationContext context, out ITranslation translation)
            {
                if (IsBooleanConstant(comparison.Right))
                {
                    translation = new StandaloneEqualityComparisonTranslation(
                        comparison.NodeType,
                        comparison.Left,
                        comparison.NodeType,
                        comparison.Right,
                        context);

                    return true;
                }

                if (IsBooleanConstant(comparison.Left))
                {
                    translation = new StandaloneEqualityComparisonTranslation(
                        comparison.NodeType,
                        comparison.Right,
                        comparison.NodeType,
                        comparison.Left,
                        context);

                    return true;
                }

                translation = null;
                return false;
            }

            private static bool IsBooleanConstant(Expression expression)
            {
                return ((expression.NodeType == Constant) || (expression.NodeType == Default)) &&
                        (expression.Type == typeof(bool));
            }

            public ExpressionType NodeType { get; }

            public Type Type => typeof(bool);

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                if (_standaloneBoolean.IsComparisonToTrue)
                {
                    _operandTranslation.WriteTo(buffer);
                    return;
                }

                NegationTranslation.ForNot(_operandTranslation).WriteTo(buffer);
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
}
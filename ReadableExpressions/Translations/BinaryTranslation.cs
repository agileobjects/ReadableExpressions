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
                [Add] = "+",
                [AddChecked] = "+",
                [And] = "&",
                [AndAlso] = "&&",
                [Coalesce] = "??",
                [Divide] = "/",
                [Equal] = "==",
                [ExclusiveOr] = "^",
                [GreaterThan] = ">",
                [GreaterThanOrEqual] = ">=",
                [LeftShift] = "<<",
                [LessThan] = "<",
                [LessThanOrEqual] = "<=",
                [Modulo] = "%",
                [Multiply] = "*",
                [MultiplyChecked] = "*",
                [NotEqual] = "!=",
                [Or] = "|",
                [OrElse] = "||",
                [Power] = "**",
                [RightShift] = ">>",
                [Subtract] = "-",
                [SubtractChecked] = "-"
            };

        private readonly ITranslation _leftOperandTranslation;
        private readonly string _operator;
        private readonly ITranslation _rightOperandTranslation;
        private readonly Action<ITranslationContext> _translationWriter;
        private readonly StandaloneBoolean _standalone;

        public BinaryTranslation(BinaryExpression binary, ITranslationContext context)
        {
            switch (binary.NodeType)
            {
                case Add:
                    break;

                case Equal:
                case NotEqual:
                    if (TryGetStandaloneBoolean(binary, out _standalone))
                    {
                        _leftOperandTranslation = context.GetTranslationFor(_standalone.Expression);
                        EstimatedSize = _leftOperandTranslation.EstimatedSize + 1;
                        _translationWriter = WriteStandaloneEqualityComparison;
                        break;
                    }

                    goto default;

                default:
                    _leftOperandTranslation = context.GetTranslationFor(binary.Left);
                    _operator = GetOperator(binary);
                    _rightOperandTranslation = context.GetTranslationFor(binary.Right);
                    _translationWriter = WriteBinary;
                    EstimatedSize = GetEstimatedSize();
                    break;
            }
        }

        public static string GetOperator(Expression expression) => _operatorsByNodeType[expression.NodeType];

        private int GetEstimatedSize()
        {
            return _leftOperandTranslation.EstimatedSize +
                   _operator.Length + 2 +
                   _rightOperandTranslation.EstimatedSize;
        }

        public int EstimatedSize { get; }

        private void WriteStandaloneEqualityComparison(ITranslationContext context)
        {
            if (_standalone.IsComparisonToTrue)
            {
                _leftOperandTranslation.WriteTo(context);
                return;
            }

            // Negation translation
        }

        private void WriteBinary(ITranslationContext context)
        {
            _leftOperandTranslation.WriteTo(context);
            context.WriteToTranslation(' ');
            context.WriteToTranslation(_operator);
            context.WriteToTranslation(' ');
            _rightOperandTranslation.WriteTo(context);
        }

        public void WriteTo(ITranslationContext context)
            => _translationWriter.Invoke(context);

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
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using BinaryExpression = Microsoft.Scripting.Ast.BinaryExpression;
    using ConstantExpression = Microsoft.Scripting.Ast.ConstantExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
    using Extensions;

    internal struct BinaryExpressionTranslator : IExpressionTranslator
    {
        private delegate string BinaryTranslator(BinaryExpression comparison, TranslationContext context);

        private static readonly Dictionary<ExpressionType, string> _operatorsByNodeType =
            new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Add] = "+",
                [ExpressionType.AddChecked] = "+",
                [ExpressionType.And] = "&",
                [ExpressionType.AndAlso] = "&&",
                [ExpressionType.Coalesce] = "??",
                [ExpressionType.Divide] = "/",
                [ExpressionType.Equal] = "==",
                [ExpressionType.ExclusiveOr] = "^",
                [ExpressionType.GreaterThan] = ">",
                [ExpressionType.GreaterThanOrEqual] = ">=",
                [ExpressionType.LeftShift] = "<<",
                [ExpressionType.LessThan] = "<",
                [ExpressionType.LessThanOrEqual] = "<=",
                [ExpressionType.Modulo] = "%",
                [ExpressionType.Multiply] = "*",
                [ExpressionType.MultiplyChecked] = "*",
                [ExpressionType.NotEqual] = "!=",
                [ExpressionType.Or] = "|",
                [ExpressionType.OrElse] = "||",
                [ExpressionType.Power] = "**",
                [ExpressionType.RightShift] = ">>",
                [ExpressionType.Subtract] = "-",
                [ExpressionType.SubtractChecked] = "-"
            };

        private static readonly ExpressionType[] _checkedOperatorTypes =
            _operatorsByNodeType.GetCheckedExpressionTypes();

        private static readonly Dictionary<ExpressionType, BinaryTranslator> _translatorsByNodeType =
            new Dictionary<ExpressionType, BinaryTranslator>
            {
                [ExpressionType.Add] = TranslateAddition,
                [ExpressionType.Equal] = TranslateEqualityComparison,
                [ExpressionType.NotEqual] = TranslateEqualityComparison
            };

        public static string GetOperator(Expression expression) => _operatorsByNodeType[expression.NodeType];

        public IEnumerable<ExpressionType> NodeTypes => _operatorsByNodeType.Keys;

        public string Translate(Expression expression, TranslationContext context)
        {
            var binary = (BinaryExpression)expression;

            return _translatorsByNodeType.TryGetValue(expression.NodeType, out var translator)
                ? translator.Invoke(binary, context)
                : Translate(binary, context);
        }

        private static string Translate(BinaryExpression binary, TranslationContext context)
        {
            var left = TranslateOperand(binary.Left, context);
            var @operator = GetOperator(binary);
            var right = TranslateOperand(binary.Right, context);

            var operation = $"{left} {@operator} {right}";

            return AdjustForCheckedOperatorIfAppropriate(binary.NodeType, operation);
        }

        private static string TranslateOperand(Expression expression, TranslationContext context)
        {
            var operand = context.TranslateCodeBlock(expression);

            if (!operand.IsASingleStatement)
            {
                return operand.WithCurlyBraces();
            }

            var translation = context.Translate(expression);

            return expression.IsAssignment()
                ? translation.WithSurroundingParentheses()
                : translation;
        }

        private static string AdjustForCheckedOperatorIfAppropriate(
            ExpressionType operatorType,
            string operation)
        {
            if (Array.IndexOf(_checkedOperatorTypes, operatorType) == -1)
            {
                return operation.WithSurroundingParentheses();
            }

            if (operation.IsMultiLine())
            {
                return $@"
checked
{{
{operation.TrimStart().Indented()}
}}".TrimStart();
            }

            return $"checked({operation})";
        }

        private static string TranslateAddition(BinaryExpression binary, TranslationContext context)
        {
            if ((binary.Left.Type != typeof(string)) && (binary.Right.Type != typeof(string)))
            {
                return Translate(binary, context);
            }

            return new[] { binary.Left, binary.Right }.ToStringConcatenation(context);
        }

        private static string TranslateEqualityComparison(BinaryExpression comparison, TranslationContext context)
        {
            return TryGetStandaloneBoolean(comparison, out var standalone)
                ? Translate(standalone, context)
                : Translate(comparison, context);
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
            return ((expression.NodeType == ExpressionType.Constant) || (expression.NodeType == ExpressionType.Default)) &&
                   (expression.Type == typeof(bool));
        }

        private static string Translate(StandaloneBoolean standalone, TranslationContext context)
        {
            return standalone.IsComparisonToTrue
                ? TranslateOperand(standalone.Boolean, context)
                : NegationExpressionTranslator.TranslateNot(standalone.Boolean, context);
        }

        private class StandaloneBoolean
        {
            public StandaloneBoolean(
                Expression boolean,
                ExpressionType @operator,
                Expression comparison)
            {
                Boolean = boolean;

                var comparisonValue =
                    (comparison.NodeType != ExpressionType.Default) &&
                    (bool)((ConstantExpression)comparison).Value;

                IsComparisonToTrue =
                    (comparisonValue && (@operator == ExpressionType.Equal)) ||
                    (!comparisonValue && (@operator == ExpressionType.NotEqual));
            }

            public Expression Boolean { get; }

            public bool IsComparisonToTrue { get; }
        }
    }
}
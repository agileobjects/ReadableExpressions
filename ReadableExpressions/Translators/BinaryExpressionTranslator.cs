namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class BinaryExpressionTranslator : ExpressionTranslatorBase
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

        private readonly NegationExpressionTranslator _negationTranslator;
        private readonly Dictionary<ExpressionType, BinaryTranslator> _translatorsByNodeType;

        internal BinaryExpressionTranslator(NegationExpressionTranslator negationTranslator)
            : base(_operatorsByNodeType.Keys.ToArray())
        {
            _negationTranslator = negationTranslator;

            _translatorsByNodeType = new Dictionary<ExpressionType, BinaryTranslator>
            {
                [ExpressionType.Add] = TranslateAddition,
                [ExpressionType.Equal] = TranslateEqualityComparison,
                [ExpressionType.NotEqual] = TranslateEqualityComparison
            };
        }

        public static string GetOperator(Expression expression) => _operatorsByNodeType[expression.NodeType];

        public override string Translate(Expression expression, TranslationContext context)
        {
            var binary = (BinaryExpression)expression;

            BinaryTranslator translator;

            return _translatorsByNodeType.TryGetValue(expression.NodeType, out translator)
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
                ? translation.WithSurroundingParentheses(checkExisting: true)
                : translation;
        }

        private static string AdjustForCheckedOperatorIfAppropriate(
            ExpressionType operatorType,
            string operation)
        {
            if (!_checkedOperatorTypes.Contains(operatorType))
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

        private string TranslateEqualityComparison(BinaryExpression comparison, TranslationContext context)
        {
            StandaloneBoolean standalone;

            return TryGetStandaloneBoolean(comparison, out standalone)
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

        private string Translate(StandaloneBoolean standalone, TranslationContext context)
        {
            return standalone.IsComparisonToTrue
                ? TranslateOperand(standalone.Boolean, context)
                : _negationTranslator.TranslateNot(standalone.Boolean, context);
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
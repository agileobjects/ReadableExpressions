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


        private readonly NegationExpressionTranslator _negationTranslator;
        private readonly Dictionary<ExpressionType, BinaryTranslator> _translatorsByNodeType;

        internal BinaryExpressionTranslator(
            NegationExpressionTranslator negationTranslator,
            Translator globalTranslator)
            : base(globalTranslator, _operatorsByNodeType.Keys.ToArray())
        {
            _negationTranslator = negationTranslator;

            _translatorsByNodeType = new Dictionary<ExpressionType, BinaryTranslator>
            {
                [ExpressionType.Equal] = TranslateEqualityComparison,
                [ExpressionType.NotEqual] = TranslateEqualityComparison,
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var binary = (BinaryExpression)expression;

            BinaryTranslator translator;

            return _translatorsByNodeType.TryGetValue(expression.NodeType, out translator)
                ? translator.Invoke(binary, context)
                : Translate(binary, context);
        }

        private string Translate(BinaryExpression binary, TranslationContext context)
        {
            var left = GetTranslation(binary.Left, context);
            var @operator = _operatorsByNodeType[binary.NodeType];
            var right = GetTranslation(binary.Right, context);

            return $"({left} {@operator} {right})";
        }

        protected override string GetTranslation(Expression expression, TranslationContext context)
        {
            var translation = base.GetTranslation(expression, context);

            if (expression.IsAssignment() && !translation.HasSurroundingParentheses())
            {
                translation = translation.WithSurroundingParentheses();
            }

            return translation;
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
                ? GetTranslation(standalone.Boolean, context)
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
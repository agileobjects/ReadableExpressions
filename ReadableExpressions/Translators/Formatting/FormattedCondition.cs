namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Linq.Expressions;

    internal class FormattedCondition : FormattableExpressionBase
    {
        private readonly Expression _condition;
        private readonly TranslationContext _context;
        private readonly string _singleLineTest;

        public FormattedCondition(
            Expression condition,
            TranslationContext context)
        {
            _condition = condition;
            _context = context;

            var test = context.TranslateAsCodeBlock(condition);

            if (test.IsMultiLine())
            {
                test = test.Indented().TrimStart();
            }

            _singleLineTest = test.WithSurroundingParentheses(CheckExistingParentheses());

            MultipleLineTranslationFactory = GetMultipleLineTranslation;
        }

        private bool CheckExistingParentheses()
        {
            // ReSharper disable once UnusedVariable
            return IsNotRelevantBinary(_condition, out BinaryExpression binary);
        }

        protected override Func<string> SingleLineTranslationFactory => () => _singleLineTest;

        protected override Func<string> MultipleLineTranslationFactory { get; }

        private string GetMultipleLineTranslation()
        {
            if (IsNotRelevantBinary(_condition, out BinaryExpression conditionBinary))
            {
                return _singleLineTest;
            }

            var conditionLeft = new FormattedCondition(conditionBinary.Left, _context);
            var conditionOperator = BinaryExpressionTranslator.GetOperator(conditionBinary);
            var conditionRight = new FormattedCondition(conditionBinary.Right, _context);

            var condition = $@"
{conditionLeft} {conditionOperator}
{conditionRight.ToString().Indented()}".TrimStart().WithSurroundingParentheses();

            return condition;
        }

        private static bool IsNotRelevantBinary(Expression condition, out BinaryExpression binary)
        {
            switch (condition.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    binary = (BinaryExpression)condition;
                    return false;
            }

            binary = null;
            return true;
        }
    }
}
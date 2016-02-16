namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ConditionalExpressionTranslator : ExpressionTranslatorBase
    {
        public ConditionalExpressionTranslator()
            : base(ExpressionType.Conditional)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var conditional = (ConditionalExpression)expression;

            var test = translatorRegistry.Translate(conditional.Test);
            var hasNoElseCondition = HasNoElseCondition(conditional);

            var ifTrue = translatorRegistry.TranslateExpressionBody(
                conditional.IfTrue,
                conditional.IfTrue.Type,
                encloseSingleStatementsInBrackets: hasNoElseCondition);

            if (hasNoElseCondition)
            {
                return IfStatement(test, ifTrue);
            }

            var ifFalse = translatorRegistry.TranslateExpressionBody(
                conditional.IfFalse,
                conditional.IfFalse.Type,
                encloseSingleStatementsInBrackets: false);

            return $"{test} ? {ifTrue} : {ifFalse}";
        }

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        private static string IfStatement(string test, string body)
        {
            return $@"if {test}{body}";
        }
    }
}
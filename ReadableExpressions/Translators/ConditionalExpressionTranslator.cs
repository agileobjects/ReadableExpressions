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

            var ifTrueBlock = translatorRegistry
                .TranslateExpressionBody(conditional.IfTrue, conditional.IfTrue.Type);

            if (hasNoElseCondition)
            {
                return IfStatement(test, ifTrueBlock.WithBrackets());
            }

            var ifFalseBlock = translatorRegistry.TranslateExpressionBody(
                conditional.IfFalse,
                conditional.IfFalse.Type);

            return $"{test} ? {ifTrueBlock.WithoutBrackets()} : {ifFalseBlock.WithoutBrackets()}";
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
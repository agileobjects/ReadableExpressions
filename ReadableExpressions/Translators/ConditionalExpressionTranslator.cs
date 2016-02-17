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

            var ifFalseBlock = translatorRegistry
                .TranslateExpressionBody(conditional.IfFalse, conditional.IfFalse.Type);

            return (conditional.Type != typeof(void))
                ? Ternary(test, ifTrueBlock, ifFalseBlock)
                : IfElseStatement(test, ifTrueBlock, ifFalseBlock);
        }

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        private static string IfStatement(string test, string body)
        {
            return $"if {test}{body}";
        }

        private static string Ternary(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            return $"{test} ? {ifTrue.WithoutBrackets()} : {ifFalse.WithoutBrackets()}";
        }

        private static string IfElseStatement(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifElse = $@"
if {test}{ifTrue.WithBrackets()}
else{ifFalse.WithBrackets()}";

            return ifElse.TrimStart();
        }
    }
}
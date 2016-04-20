namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Formatting;

    internal class ConditionalExpressionTranslator : ExpressionTranslatorBase
    {
        public ConditionalExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Conditional)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var conditional = (ConditionalExpression)expression;

            var test = GetTest(GetTranslation(conditional.Test, context));
            var hasNoElseCondition = HasNoElseCondition(conditional);

            var ifTrueBlock = GetTranslatedExpressionBody(conditional.IfTrue, context);

            if (hasNoElseCondition)
            {
                return IfStatement(test, ifTrueBlock.WithParentheses());
            }

            var ifFalseBlock = GetTranslatedExpressionBody(conditional.IfFalse, context);

            if (IsSuitableForTernary(conditional, ifTrueBlock, ifFalseBlock))
            {
                return new FormattableTernaryExpression(test, ifTrueBlock, ifFalseBlock);
            }

            if (conditional.IfTrue.IsReturnable())
            {
                return ShortCircuitingIf(test, ifTrueBlock, ifFalseBlock);
            }

            return IfElse(test, ifTrueBlock, ifFalseBlock, IsElseIf(conditional));
        }

        private static string GetTest(string test)
        {
            return test.StartsWith('(') ? test : $"({test})";
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

        private static bool IsSuitableForTernary(Expression conditional, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            return (conditional.Type != typeof(void)) &&
                   ifTrue.IsASingleStatement &&
                   ifFalse.IsASingleStatement;
        }

        private static string ShortCircuitingIf(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifElseBlock = $@"
if {test}{ifTrue.WithParentheses()}

{ifFalse.WithoutParentheses()}";

            return ifElseBlock.TrimStart();
        }

        private static bool IsElseIf(ConditionalExpression conditional)
        {
            return conditional.IfFalse.NodeType == ExpressionType.Conditional;
        }

        private static string IfElse(
            string test,
            CodeBlock ifTrue,
            CodeBlock ifFalse,
            bool isElseIf)
        {
            var ifFalseBlock = isElseIf
                ? " " + ifFalse.WithoutParentheses()
                : ifFalse.WithParentheses();

            string ifElseBlock = $@"
if {test}{ifTrue.WithParentheses()}
else{ifFalseBlock}";


            return ifElseBlock.TrimStart();
        }
    }
}
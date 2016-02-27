namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ConditionalExpressionTranslator : ExpressionTranslatorBase
    {
        public ConditionalExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Conditional)
        {
        }

        public override string Translate(Expression expression)
        {
            var conditional = (ConditionalExpression)expression;

            var test = GetTest(Registry.Translate(conditional.Test));
            var hasNoElseCondition = HasNoElseCondition(conditional);

            var ifTrueBlock = Registry.TranslateExpressionBody(conditional.IfTrue);

            if (hasNoElseCondition)
            {
                return IfStatement(test, ifTrueBlock.WithBrackets());
            }

            var ifFalseBlock = Registry.TranslateExpressionBody(conditional.IfFalse);

            if (IsSuitableForTernary(conditional, ifTrueBlock, ifFalseBlock))
            {
                return Ternary(test, ifTrueBlock, ifFalseBlock);
            }

            if (conditional.IfTrue.Type != typeof(void))
            {
                return ShortCircuitingIf(test, ifTrueBlock, ifFalseBlock);
            }

            return IfElse(test, ifTrueBlock, ifFalseBlock, IsElseIf(conditional));
        }

        private static string GetTest(string test)
        {
            return test.StartsWith("(", StringComparison.Ordinal) ? test : $"({test})";
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

        private static string Ternary(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            return $"{test} ?{ifTrue.AsExpressionBody()} :{ifFalse.AsExpressionBody()}";
        }

        private static string ShortCircuitingIf(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifElseBlock = $@"
if {test}{ifTrue.WithBrackets()}

{ifFalse.WithoutBrackets()}";

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
                ? " " + ifFalse.WithoutBrackets()
                : ifFalse.WithBrackets();

            string ifElseBlock = $@"
if {test}{ifTrue.WithBrackets()}
else{ifFalseBlock}";


            return ifElseBlock.TrimStart();
        }
    }
}
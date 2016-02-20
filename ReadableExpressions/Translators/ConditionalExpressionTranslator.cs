namespace AgileObjects.ReadableExpressions.Translators
{
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

            return IsSuitableForTernary(conditional, ifTrueBlock, ifFalseBlock)
                ? Ternary(test, ifTrueBlock, ifFalseBlock)
                : IfElseStatement(test, ifTrueBlock, ifFalseBlock, IsElseIf(conditional));
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
            return $"{test} ? {ifTrue.AsExpressionBody()} : {ifFalse.AsExpressionBody()}";
        }

        private static bool IsElseIf(ConditionalExpression conditional)
        {
            return conditional.IfFalse.NodeType == ExpressionType.Conditional;
        }

        private static string IfElseStatement(
            string test,
            CodeBlock ifTrue,
            CodeBlock ifFalse,
            bool isElseIf)
        {
            string ifElseBlock;

            if (ifTrue.ReturnType != typeof(void))
            {
                ifElseBlock = $@"
if {test}{ifTrue.WithBrackets()}

{ifFalse.WithoutBrackets()}";
            }
            else
            {
                var ifFalseBlock = isElseIf
                    ? " " + ifFalse.WithoutBrackets()
                    : ifFalse.WithBrackets();

                ifElseBlock = $@"
if {test}{ifTrue.WithBrackets()}
else{ifFalseBlock}";
            }

            return ifElseBlock.TrimStart();
        }
    }
}
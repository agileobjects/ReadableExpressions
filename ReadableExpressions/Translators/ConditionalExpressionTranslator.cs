namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;
    using Formatting;

    internal class ConditionalExpressionTranslator : ExpressionTranslatorBase
    {
        public ConditionalExpressionTranslator()
            : base(ExpressionType.Conditional)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var conditional = (ConditionalExpression)expression;

            var test = GetTest(context.GetTranslation(conditional.Test));
            var hasNoElseCondition = HasNoElseCondition(conditional);

            var ifTrueBlock = GetTranslatedExpressionBody(conditional.IfTrue, context);

            if (hasNoElseCondition)
            {
                return IfStatement(test, ifTrueBlock.WithCurlyBraces());
            }

            var ifFalseBlock = GetTranslatedExpressionBody(conditional.IfFalse, context);

            if (IsTernary(conditional))
            {
                return new TernaryExpression(test, ifTrueBlock, ifFalseBlock);
            }

            if (conditional.IfTrue.IsReturnable())
            {
                return ShortCircuitingIf(test, ifTrueBlock, ifFalseBlock);
            }

            return IfElse(test, ifTrueBlock, ifFalseBlock, IsElseIf(conditional));
        }

        private static string GetTest(string test)
        {
            return (test.StartsWith('(') && test.EndsWith(')')) ? test : test.WithSurroundingParentheses();
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

        private static bool IsTernary(Expression conditional)
        {
            return conditional.Type != typeof(void);
        }

        private static string ShortCircuitingIf(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifElseBlock = $@"
if {test}{ifTrue.WithCurlyBraces()}

{ifFalse.WithoutCurlyBraces()}";

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
                ? " " + ifFalse.WithoutCurlyBraces()
                : ifFalse.WithCurlyBraces();

            string ifElseBlock = $@"
if {test}{ifTrue.WithCurlyBraces()}
else{ifFalseBlock}";


            return ifElseBlock.TrimStart();
        }
    }
}
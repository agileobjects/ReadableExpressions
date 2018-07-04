namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET35
    using System.Linq.Expressions;
#else
    using ConditionalExpression = Microsoft.Scripting.Ast.ConditionalExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
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

            var hasNoElseCondition = HasNoElseCondition(conditional);

            var ifTrueBlock = context.TranslateCodeBlock(conditional.IfTrue);

            if (hasNoElseCondition)
            {
                return IfStatement(GetTest(conditional, context), ifTrueBlock.WithCurlyBraces());
            }

            var ifFalseBlock = context.TranslateCodeBlock(conditional.IfFalse);

            if (IsTernary(conditional))
            {
                return new FormattedTernary(conditional.Test, ifTrueBlock, ifFalseBlock, context);
            }

            var test = GetTest(conditional, context);

            if (conditional.IfTrue.IsReturnable())
            {
                return ShortCircuitingIf(test, ifTrueBlock, ifFalseBlock);
            }

            return IfElse(test, ifTrueBlock, ifFalseBlock, IsElseIf(conditional));
        }

        private static string GetTest(ConditionalExpression conditional, TranslationContext context)
            => new FormattedCondition(conditional.Test, context);

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        private static string IfStatement(string test, string body) => $"if {test}{body}";

        private static bool IsTernary(Expression conditional) => conditional.Type != typeof(void);

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

            var ifElseBlock = $@"
if {test}{ifTrue.WithCurlyBraces()}
else{ifFalseBlock}";


            return ifElseBlock.TrimStart();
        }
    }
}
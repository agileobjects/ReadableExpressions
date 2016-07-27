namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Formatting;

    internal class SwitchExpressionTranslator : ExpressionTranslatorBase
    {
        public SwitchExpressionTranslator()
            : base(ExpressionType.Switch)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var switchStatement = (SwitchExpression)expression;

            var switchValue = context.Translate(switchStatement.SwitchValue);

            var switchCases = switchStatement.Cases
                .Select(@case => new
                {
                    Tests = @case.TestValues.Select(value => $"case {context.Translate(value)}:"),
                    BodyBlock = context.TranslateCodeBlock(@case.Body)
                })
                .Select(@case => GetCase(@case.BodyBlock, @case.Tests.ToArray()));

            switchCases = AppendDefaultCaseIfExists(switchCases, switchStatement.DefaultBody, context);

            var switchCaseLines = string.Join(Environment.NewLine + Environment.NewLine, switchCases);

            var @switch = $@"
switch ({switchValue})
{{
{switchCaseLines}
}}";

            return @switch.TrimStart();
        }

        private static string GetCase(CodeBlock bodyBlock, params string[] labels)
        {
            if (!bodyBlock.HasReturn())
            {
                bodyBlock = bodyBlock.Append("break;");
            }

            var @case = bodyBlock.Indented().Insert(labels).Indented();

            return @case.WithoutCurlyBraces();
        }

        private IEnumerable<string> AppendDefaultCaseIfExists(
            IEnumerable<string> switchCases,
            Expression defaultBody,
            TranslationContext context)
        {
            foreach (var switchCase in switchCases)
            {
                yield return switchCase;
            }

            if (defaultBody != null)
            {
                yield return GetDefaultCase(defaultBody, context);
            }
        }

        private static string GetDefaultCase(Expression defaultBody, TranslationContext context)
        {
            var defaultCaseBody = context.TranslateCodeBlock(defaultBody);

            return GetCase(defaultCaseBody, "default:");
        }
    }
}
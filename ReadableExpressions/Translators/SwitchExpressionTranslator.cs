namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Formatting;

    internal class SwitchExpressionTranslator : ExpressionTranslatorBase
    {
        public SwitchExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Switch)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var switchStatement = (SwitchExpression)expression;

            var switchValue = GetTranslation(switchStatement.SwitchValue, context);

            var switchCases = switchStatement.Cases
                .Select(@case => new
                {
                    Tests = @case.TestValues.Select(value => $"case {GetTranslation(value, context)}:"),
                    BodyBlock = GetTranslatedExpressionBody(@case.Body, context)
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

            return @case.WithoutParentheses();
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

        private string GetDefaultCase(Expression defaultBody, TranslationContext context)
        {
            var defaultCaseBody = GetTranslatedExpressionBody(defaultBody, context);

            return GetCase(defaultCaseBody, "default:");
        }
    }
}
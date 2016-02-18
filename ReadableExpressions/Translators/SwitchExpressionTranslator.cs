namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class SwitchExpressionTranslator : ExpressionTranslatorBase
    {
        public SwitchExpressionTranslator()
            : base(ExpressionType.Switch)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var switchStatement = (SwitchExpression)expression;

            var switchValue = translatorRegistry.Translate(switchStatement.SwitchValue);

            var switchCases = switchStatement.Cases
                .Select(@case => new
                {
                    Tests = @case.TestValues.Select(value => $"case {translatorRegistry.Translate(value)}:"),
                    BodyBlock = translatorRegistry.TranslateExpressionBody(@case.Body)
                })
                .Select(@case => GetCase(@case.BodyBlock, @case.Tests.ToArray()));

            switchCases = AppendDefaultCaseIfExists(switchCases, switchStatement.DefaultBody, translatorRegistry);

            var switchCaseLines = string.Join(
                Environment.NewLine + Environment.NewLine,
                switchCases);

            var @switch = $@"
switch ({switchValue})
{{
{switchCaseLines}
}}";

            return @switch.TrimStart();
        }

        private static string GetCase(CodeBlock bodyBlock, params string[] labels)
        {
            var caseBody = bodyBlock.Indented().WithoutBrackets();

            var caseBlock = new CodeBlock(
                bodyBlock.ReturnType,
                labels.Concat(new[] { caseBody }).ToArray());

            var @case = caseBlock.Indented().WithoutBrackets();

            return @case;
        }

        private static IEnumerable<string> AppendDefaultCaseIfExists(
            IEnumerable<string> switchCases,
            Expression defaultBody,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            foreach (var switchCase in switchCases)
            {
                yield return switchCase;
            }

            if (defaultBody != null)
            {
                yield return GetDefaultCase(defaultBody, translatorRegistry);
            }
        }

        private static string GetDefaultCase(
            Expression defaultBody,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var defaultCaseBody = translatorRegistry.TranslateExpressionBody(defaultBody);

            return GetCase(defaultCaseBody, "default:");
        }
    }
}
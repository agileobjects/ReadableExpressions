namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Formatting;

    internal class SwitchExpressionTranslator : ExpressionTranslatorBase
    {
        public SwitchExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Switch)
        {
        }

        public override string Translate(Expression expression)
        {
            var switchStatement = (SwitchExpression)expression;

            var switchValue = GetTranslation(switchStatement.SwitchValue);

            var switchCases = switchStatement.Cases
                .Select(@case => new
                {
                    Tests = @case.TestValues.Select(value => $"case {GetTranslation(value)}:"),
                    BodyBlock = GetTranslatedExpressionBody(@case.Body)
                })
                .Select(@case => GetCase(@case.BodyBlock, @case.Tests.ToArray()));

            switchCases = AppendDefaultCaseIfExists(switchCases, switchStatement.DefaultBody);

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
            var caseBody = bodyBlock.Indented().WithoutBrackets();
            var caseBlock = new CodeBlock(labels.Concat(new[] { caseBody }).ToArray());
            var @case = caseBlock.Indented().WithoutBrackets();

            return @case;
        }

        private IEnumerable<string> AppendDefaultCaseIfExists(
            IEnumerable<string> switchCases,
            Expression defaultBody)
        {
            foreach (var switchCase in switchCases)
            {
                yield return switchCase;
            }

            if (defaultBody != null)
            {
                yield return GetDefaultCase(defaultBody);
            }
        }

        private string GetDefaultCase(Expression defaultBody)
        {
            var defaultCaseBody = GetTranslatedExpressionBody(defaultBody);

            return GetCase(defaultCaseBody, "default:");
        }
    }
}
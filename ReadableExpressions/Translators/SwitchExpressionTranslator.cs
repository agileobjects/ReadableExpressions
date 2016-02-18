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
                .Select(@case => GetCaseStatement(@case.Tests, @case.BodyBlock));

            var switchCaseLines = string.Join(Environment.NewLine, switchCases);

            var @switch = $@"
switch ({switchValue})
{{
{switchCaseLines}
}}";

            return @switch.TrimStart();
        }

        private static string GetCaseStatement(IEnumerable<string> tests, CodeBlock bodyBlock)
        {
            var body = bodyBlock.IsASingleStatement
                ? bodyBlock.Indented().WithoutBrackets()
                : bodyBlock.WithBrackets();

            var caseBlock = new CodeBlock(
                bodyBlock.ReturnType,
                tests.Concat(new[] { body }).ToArray());

            var caseStatement = caseBlock.Indented().WithoutBrackets();

            return caseStatement;
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;

    internal class TernaryExpression : FormattableExpressionBase
    {
        public TernaryExpression(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifTrueString = GetBranch(ifTrue);
            var ifFalseString = GetBranch(ifFalse);

            SingleLineTranslationFactory = () => $"{test} ?{ifTrueString} :{ifFalseString}";

            MultipleLineTranslationFactory = () =>
                test +
                Environment.NewLine +
                ("?" + ifTrueString).Indent() +
                Environment.NewLine +
                (":" + ifFalseString).Indent();
        }

        private static string GetBranch(CodeBlock codeBlock)
        {
            return codeBlock.IsASingleStatement
                ? codeBlock.AsExpressionBody()
                : " " + codeBlock.WithCurlyBraces().TrimStart();
        }

        protected override Func<string> SingleLineTranslationFactory { get; }

        protected override Func<string> MultipleLineTranslationFactory { get; }
    }
}
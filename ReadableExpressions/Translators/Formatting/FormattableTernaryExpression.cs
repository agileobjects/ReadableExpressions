namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;

    internal class FormattableTernaryExpression : FormattableExpressionBase
    {
        public FormattableTernaryExpression(string test, CodeBlock ifTrue, CodeBlock ifFalse)
        {
            var ifTrueString = ifTrue.AsExpressionBody();
            var ifFalseString = ifFalse.AsExpressionBody();

            SingleLineTranslationFactory = () => $"{test} ?{ifTrueString} :{ifFalseString}";

            MultipleLineTranslationFactory = () =>
                test +
                Environment.NewLine +
                ("?" + ifTrueString).Indent() +
                Environment.NewLine +
                (":" + ifFalseString).Indent();
        }

        protected override Func<string> SingleLineTranslationFactory { get; }

        protected override Func<string> MultipleLineTranslationFactory { get; }
    }
}
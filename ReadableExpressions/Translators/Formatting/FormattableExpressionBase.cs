namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using Extensions;

    internal abstract class FormattableExpressionBase
    {
        private const int MaxLineLength = 100;

        public static implicit operator string(FormattableExpressionBase expression)
        {
            return expression.ToString();
        }

        protected abstract Func<string> SingleLineTranslationFactory { get; }

        protected abstract Func<string> MultipleLineTranslationFactory { get; }

        protected string GetFormattedTranslation()
        {
            var translation = SingleLineTranslationFactory.Invoke();

            return SplitToMultipleLines(translation)
                ? MultipleLineTranslationFactory.Invoke()
                : translation;
        }

        protected virtual bool SplitToMultipleLines(string translation)
        {
            return (translation.Length > MaxLineLength) || translation.IsMultiLine();
        }

        public override string ToString()
        {
            return GetFormattedTranslation();
        }
    }
}
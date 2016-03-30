namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;

    internal abstract class FormattableExpressionBase
    {
        public static implicit operator string(FormattableExpressionBase expression)
        {
            return expression.ToString();
        }

        protected abstract Func<string> SingleLineTranslationFactory { get; }

        protected abstract Func<string> MultipleLineTranslationFactory { get; }

        public override string ToString()
        {
            var translation = SingleLineTranslationFactory.Invoke();

            return (translation.Length > 100) || translation.Contains(Environment.NewLine)
                ? MultipleLineTranslationFactory.Invoke()
                : translation;
        }
    }
}
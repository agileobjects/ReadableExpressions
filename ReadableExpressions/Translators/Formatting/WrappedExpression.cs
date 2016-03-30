namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Linq.Expressions;

    internal class WrappedExpression : FormattableExpressionBase
    {
        public WrappedExpression(Expression expression, IExpressionTranslatorRegistry registry)
        {
            SingleLineTranslationFactory = () => registry.Translate(expression);
        }

        protected override Func<string> SingleLineTranslationFactory { get; }

        protected override Func<string> MultipleLineTranslationFactory => SingleLineTranslationFactory;
    }
}
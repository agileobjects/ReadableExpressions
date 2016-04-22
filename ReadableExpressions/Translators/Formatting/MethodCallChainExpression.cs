namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class MethodCallChainExpression : FormattableExpressionBase
    {
        private readonly IEnumerable<MethodCallExpression> _methodCalls;
        private readonly Func<MethodCallExpression, string> _methodCallTranslator;

        public MethodCallChainExpression(
            IEnumerable<MethodCallExpression> methodCalls,
            TranslationContext context,
            Translator globalTranslator)
        {
            _methodCalls = methodCalls;
            _methodCallTranslator = mc => globalTranslator.Invoke(mc, context);

            SingleLineTranslationFactory = () => FormatMethodCalls(".");

            MultipleLineTranslationFactory = () =>
                FormatMethodCalls(Environment.NewLine + ".", mc => mc.Indent());
        }

        private string FormatMethodCalls(
            string separator,
            Func<string, string> extraFormatter = null)
        {
            if (extraFormatter == null)
            {
                extraFormatter = s => s;
            }

            return string.Join(
                separator,
                _methodCalls.Select(_methodCallTranslator).Select(extraFormatter));
        }

        protected override Func<string> SingleLineTranslationFactory { get; }

        protected override Func<string> MultipleLineTranslationFactory { get; }
    }
}
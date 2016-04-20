namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<string> _parameterModifiers;
        private readonly IEnumerable<Expression> _arguments;
        private readonly TranslationContext _context;
        private readonly Translator _globalTranslator;

        public ParameterSet(
            IMethodInfo method,
            IEnumerable<Expression> arguments,
            TranslationContext context,
            Translator globalTranslator)
        {
            _parameterModifiers = GetParameterModifers(method);
            _arguments = arguments;
            _context = context;
            _globalTranslator = globalTranslator;
        }

        private static IEnumerable<string> GetParameterModifers(IMethodInfo method)
        {
            if (method == null)
            {
                return Enumerable.Empty<string>();
            }

            return method
                .GetParameters()
                .Select(p => p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : null)
                .ToArray();
        }

        protected override Func<string> SingleLineTranslationFactory => () => FormatParameters(", ");

        protected override Func<string> MultipleLineTranslationFactory
        {
            get
            {
                return () =>
                    Environment.NewLine +
                    FormatParameters("," + Environment.NewLine, a => a.Indent());
            }
        }

        private string FormatParameters(
            string separator,
            Func<string, string> extraFormatter = null)
        {
            if (extraFormatter == null)
            {
                extraFormatter = s => s;
            }

            return string.Join(
                separator,
                _arguments
                    .Select(TranslateArgument)
                    .Select(extraFormatter));
        }

        private string TranslateArgument(Expression argument, int parameterIndex)
        {
            var modifier = _parameterModifiers.ElementAtOrDefault(parameterIndex);
            var argumentString = _globalTranslator.Invoke(argument, _context).Unterminated();

            return modifier + argumentString;
        }

        public string WithParenthesesIfNecessary()
        {
            return (_arguments.Count() == 1) ? WithoutParentheses() : WithParentheses();
        }

        public string WithoutParentheses()
        {
            return GetFormattedTranslation();
        }

        public string WithParentheses()
        {
            return _arguments.Any() ? GetFormattedTranslation().WithSurroundingParentheses() : "()";
        }
    }
}
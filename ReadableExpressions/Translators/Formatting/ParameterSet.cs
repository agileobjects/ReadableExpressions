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
        private readonly Func<Expression, TranslationContext, string> _globalTranslator;

        public ParameterSet(
            IMethodInfo method,
            IEnumerable<Expression> arguments,
            TranslationContext context,
            Func<Expression, TranslationContext, string> globalTranslator)
        {
            _parameterModifiers = method?.GetParameters().Select(p => p.IsOut ? "out " : null) ?? Enumerable.Empty<string>();
            _arguments = arguments;
            _context = context;
            _globalTranslator = globalTranslator;
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

            return modifier + _globalTranslator.Invoke(argument, _context);
        }

        public string WithBracketsIfNecessary()
        {
            return (_arguments.Count() == 1) ? WithoutBrackets() : WithBrackets();
        }

        public string WithoutBrackets()
        {
            return ToString();
        }

        public string WithBrackets()
        {
            return _arguments.Any() ? $"({ToString()})" : "()";
        }
    }
}
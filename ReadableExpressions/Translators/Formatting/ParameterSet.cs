namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<Expression> _parameters;
        private readonly IExpressionTranslatorRegistry _registry;

        public ParameterSet(IEnumerable<Expression> parameters, IExpressionTranslatorRegistry registry)
        {
            _parameters = parameters;
            _registry = registry;
        }

        protected override Func<string> SingleLineTranslationFactory => () => FormatParameters(", ");

        protected override Func<string> MultipleLineTranslationFactory
        {
            get
            {
                return () =>
                    Environment.NewLine +
                    FormatParameters("," + Environment.NewLine, p => p.Indent());
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

            return string.Join(separator, _parameters.Select(_registry.Translate).Select(extraFormatter));
        }

        public string WithBracketsIfNecessary()
        {
            return (_parameters.Count() == 1) ? WithoutBrackets() : WithBrackets();
        }

        public string WithoutBrackets()
        {
            return ToString();
        }

        public string WithBrackets()
        {
            return _parameters.Any() ? $"({ToString()})" : "()";
        }
    }
}
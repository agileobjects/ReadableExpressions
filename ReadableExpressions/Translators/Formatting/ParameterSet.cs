namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<Expression> _parameters;
        private readonly Func<Expression, string> _translator;

        public ParameterSet(IEnumerable<Expression> parameters, Func<Expression, string> translator)
        {
            _parameters = parameters;
            _translator = translator;
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

            return string.Join(separator, _parameters.Select(_translator.Invoke).Select(extraFormatter));
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
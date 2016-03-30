namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<FormattableExpressionBase> _parameters;

        public ParameterSet(IEnumerable<Expression> parameters, IExpressionTranslatorRegistry registry)
        {
            _parameters = parameters
                .Select(arg => new WrappedExpression(arg, registry))
                .ToArray();
        }

        protected override Func<string> SingleLineTranslationFactory => () => string.Join(", ", _parameters);

        protected override Func<string> MultipleLineTranslationFactory
        {
            get
            {
                return () =>
                    Environment.NewLine +
                    string.Join(
                        "," + Environment.NewLine,
                        _parameters.Select(arg => arg.ToString().Indent()));
            }
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
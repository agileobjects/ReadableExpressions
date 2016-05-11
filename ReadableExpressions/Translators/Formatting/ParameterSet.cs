namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ParameterSet : FormattableExpressionBase
    {
        private readonly IEnumerable<Func<string, string>> _parameterModifiers;
        private readonly IEnumerable<Expression> _arguments;
        private readonly Func<Expression, string> _argumentTranslator;

        public ParameterSet(
            IMethodInfo method,
            IEnumerable<Expression> arguments,
            TranslationContext context,
            Translator globalTranslator)
        {
            _parameterModifiers = GetParameterModifers(method);
            _arguments = arguments;
            _argumentTranslator = arg => globalTranslator.Invoke(arg, context);
        }

        private static IEnumerable<Func<string, string>> GetParameterModifers(IMethodInfo method)
        {
            if (method == null)
            {
                return Enumerable.Empty<Func<string, string>>();
            }

            return method
                .GetParameters()
                .Select(GetParameterModifier)
                .ToArray();
        }

        private static Func<string, string> GetParameterModifier(ParameterInfo parameter)
        {
            if (parameter.IsOut)
            {
                return p => "out " + p;
            }

            if (parameter.ParameterType.IsByRef)
            {
                return p => "ref " + p;
            }

            if (Attribute.IsDefined(parameter, typeof(ParamArrayAttribute)))
            {
                return FormatParamsArray;
            }

            return default(Func<string, string>);
        }

        private static string FormatParamsArray(string array)
        {
            var arrayValuesStart = array.IndexOf('{') + 1;
            var arrayValuesEnd = array.LastIndexOf('}');
            var arrayValues = array.Substring(arrayValuesStart, arrayValuesEnd - arrayValuesStart).Trim();

            return arrayValues;
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
                    .Select(extraFormatter)
                    .Where(arg => arg != string.Empty));
        }

        private string TranslateArgument(Expression argument, int parameterIndex)
        {
            var argumentString = _argumentTranslator.Invoke(argument).Unterminated();
            var modifier = _parameterModifiers.ElementAtOrDefault(parameterIndex);

            return (modifier != null) ? modifier.Invoke(argumentString) : argumentString;
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
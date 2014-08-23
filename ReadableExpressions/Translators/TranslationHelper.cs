namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class TranslationHelper
    {
        internal static string GetParameters<TExpression>(
            IEnumerable<TExpression> parameters,
            IExpressionTranslatorRegistry translatorRegistry,
            bool encloseSingleParameterInBrackets)
            where TExpression : Expression
        {
            if (!parameters.Any())
            {
                return "()";
            }

            var parametersString = string.Join(
                ", ",
                parameters.Select(translatorRegistry.Translate));

            if (encloseSingleParameterInBrackets || (parameters.Count() > 1))
            {
                parametersString = "(" + parametersString + ")";
            }

            return parametersString;
        }
    }
}

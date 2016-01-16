namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class TranslationHelper
    {
        internal static string GetParameters<TExpression>(
            IEnumerable<TExpression> parameters,
            IExpressionTranslatorRegistry translatorRegistry,
            bool placeLongListsOnMultipleLines,
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

            if (placeLongListsOnMultipleLines && (parametersString.Length > 100))
            {
                var indent = Environment.NewLine + "    ";
                parametersString = indent + parametersString.Replace(", ", "," + indent);
            }

            if (encloseSingleParameterInBrackets || (parameters.Count() > 1))
            {
                parametersString = "(" + parametersString + ")";
            }

            return parametersString;
        }
    }
}

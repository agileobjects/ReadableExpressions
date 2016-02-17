namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class TranslatorRegistryExtensions
    {
        public static string TranslateParameters<TExpression>(
            this IExpressionTranslatorRegistry translatorRegistry,
            IEnumerable<TExpression> parameters,
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

        public static CodeBlock TranslateExpressionBody(
            this IExpressionTranslatorRegistry translatorRegistry,
            Expression body,
            Type returnType)
        {
            var codeBlock = TranslateBlock(body as BlockExpression, returnType, translatorRegistry)
                ?? TranslateSingle(body, translatorRegistry);

            return codeBlock;
        }

        #region TranslateExpressionBody

        private static CodeBlock TranslateBlock(
            BlockExpression bodyBlock,
            Type returnType,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            if (bodyBlock == null)
            {
                return null;
            }

            if (bodyBlock.Expressions.Count == 1)
            {
                return TranslateSingle(bodyBlock, translatorRegistry);
            }

            var block = translatorRegistry.Translate(bodyBlock);
            var blockLines = block.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (blockLines.Length == 1)
            {
                return new CodeBlock(returnType, blockLines.First());
            }

            if (returnType != typeof(void))
            {
                blockLines[blockLines.Length - 1] = "return " + blockLines[blockLines.Length - 1];
            }

            return new CodeBlock(returnType, blockLines);
        }

        private static CodeBlock TranslateSingle(
            Expression bodySingle,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var body = translatorRegistry.Translate(bodySingle);

            if (body != null)
            {
                if (body.StartsWith("(", StringComparison.Ordinal) &&
                    body.EndsWith(")", StringComparison.Ordinal))
                {
                    body = body.Substring(1, body.Length - 2);
                }
            }

            return new CodeBlock(bodySingle.Type, body);
        }

        #endregion
    }
}

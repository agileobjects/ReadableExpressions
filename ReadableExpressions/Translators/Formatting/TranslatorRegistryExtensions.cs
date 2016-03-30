namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal static class FormattingExtensions
    {
        public static ParameterSet TranslateParameters(
            this IExpressionTranslatorRegistry translatorRegistry,
            IEnumerable<Expression> parameters)
        {
            return new ParameterSet(parameters, translatorRegistry);
        }

        public static CodeBlock TranslateExpressionBody(
            this IExpressionTranslatorRegistry translatorRegistry,
            Expression body)
        {
            var codeBlock = TranslateBlock(body as BlockExpression, translatorRegistry)
                ?? TranslateSingle(body, translatorRegistry);

            return codeBlock;
        }

        #region TranslateExpressionBody

        private static CodeBlock TranslateBlock(
            BlockExpression bodyBlock,
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
            var blockLines = block.SplitToLines();

            return new CodeBlock(blockLines);
        }

        private static CodeBlock TranslateSingle(
            Expression bodySingle,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var body = translatorRegistry.Translate(bodySingle);

            if ((body?.StartsWith("(", StringComparison.Ordinal) == true) &&
                body.EndsWith(")", StringComparison.Ordinal))
            {
                body = body.Substring(1, body.Length - 2);
            }

            return new CodeBlock(body);
        }

        #endregion
    }
}

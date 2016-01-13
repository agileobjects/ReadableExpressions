namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator()
            : base(ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = TranslationHelper.GetParameters(
                lambda.Parameters,
                translatorRegistry,
                encloseSingleParameterInBrackets: false);

            var body = TranslateBody(lambda, translatorRegistry);

            return parameters + " =>" + body;
        }

        private static string TranslateBody(LambdaExpression lambda, IExpressionTranslatorRegistry translatorRegistry)
        {
            return TranslateBlock(lambda.Body as BlockExpression, lambda.ReturnType, translatorRegistry)
                ?? TranslateSingle(lambda.Body, translatorRegistry);
        }

        private static string TranslateBlock(
            BlockExpression lambdaBlock,
            Type returnType,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            if (lambdaBlock == null)
            {
                return null;
            }

            if (lambdaBlock.Expressions.Count == 1)
            {
                return TranslateSingle(lambdaBlock, translatorRegistry);
            }

            var block = translatorRegistry.Translate(lambdaBlock);
            var blockLines = block.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (returnType != typeof(void))
            {
                blockLines[blockLines.Length - 1] = "return " + blockLines[blockLines.Length - 1];
            }

            var indentedBlock = string.Join(Environment.NewLine, blockLines.Select(line => "    " + line));

            var bracketedBlock = $@"
{{
{indentedBlock}
}}";
            return bracketedBlock;

        }

        private static string TranslateSingle(Expression lambdaSingle, IExpressionTranslatorRegistry translatorRegistry)
        {
            var body = translatorRegistry.Translate(lambdaSingle);

            if ((body[0] == '(') && (body[body.Length - 1] == ')'))
            {
                body = body.Substring(1, body.Length - 2);
            }

            return " " + body;
        }
    }
}
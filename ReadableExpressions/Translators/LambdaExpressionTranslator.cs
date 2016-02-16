namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
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

            var parameters = translatorRegistry.TranslateParameters(
                lambda.Parameters,
                placeLongListsOnMultipleLines: false,
                encloseSingleParameterInBrackets: false);

            var body = translatorRegistry.TranslateExpressionBody(
                lambda.Body,
                lambda.ReturnType,
                encloseSingleStatementsInBrackets: false);

            if (!body.Contains(Environment.NewLine))
            {
                body = " " + body;
            }

            return parameters + " =>" + body;
        }
    }
}
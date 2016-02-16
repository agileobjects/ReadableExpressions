namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator()
            : base(ExpressionType.New)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var newExpression = (NewExpression)expression;

            var parameters = translatorRegistry.TranslateParameters(
                newExpression.Arguments,
                placeLongListsOnMultipleLines: true,
                encloseSingleParameterInBrackets: true);

            return "new " + expression.Type.GetFriendlyName() + parameters;
        }
    }
}
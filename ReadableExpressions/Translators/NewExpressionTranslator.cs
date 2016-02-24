namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.New)
        {
        }

        public override string Translate(Expression expression)
        {
            var newExpression = (NewExpression)expression;

            var parameters = Registry.TranslateParameters(
                newExpression.Arguments,
                placeLongListsOnMultipleLines: true,
                encloseSingleParameterInBrackets: true);

            return "new " + expression.Type.GetFriendlyName() + parameters;
        }
    }
}
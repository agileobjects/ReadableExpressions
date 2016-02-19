namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class GotoExpressionTranslator : ExpressionTranslatorBase
    {
        public GotoExpressionTranslator()
            : base(ExpressionType.Goto)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var gotoExpression = (GotoExpression)expression;

            return gotoExpression.Kind.ToString().ToLowerInvariant();
        }
    }
}
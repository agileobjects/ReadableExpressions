namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ExtensionExpressionTranslator : ExpressionTranslatorBase
    {
        public ExtensionExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Extension)
        {
        }

        public override string Translate(Expression expression)
        {
            return expression.ToString();
        }
    }
}
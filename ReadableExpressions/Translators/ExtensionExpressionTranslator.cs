namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ExtensionExpressionTranslator : ExpressionTranslatorBase
    {
        public ExtensionExpressionTranslator()
            : base(ExpressionType.Extension)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return expression.ToString();
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class DefaultExpressionTranslator : ExpressionTranslatorBase
    {
        public DefaultExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Default)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var defaultExpression = (DefaultExpression)expression;

            if (defaultExpression.Type == typeof(void))
            {
                return null;
            }

            return defaultExpression.Type.CanBeNull()
                ? "null"
                : Translate(defaultExpression);
        }

        internal string Translate(DefaultExpression defaultExpression)
        {
            var typeName = defaultExpression.Type.GetFriendlyName();

            return $"default({typeName})";
        }
    }
}
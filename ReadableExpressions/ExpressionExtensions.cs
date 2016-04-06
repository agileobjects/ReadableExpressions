namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        public static string ToReadableString(this Expression expression)
        {
            return _translatorRegistry
                .Translate(expression, new TranslationContext(expression))?
                .WithoutUnindents();
        }
    }

    public class TranslationContext
    {
        private readonly Expression _rootExpression;

        internal TranslationContext(Expression rootExpression)
        {
            _rootExpression = rootExpression;
        }
    }
}

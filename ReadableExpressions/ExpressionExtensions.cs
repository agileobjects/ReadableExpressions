namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        public static string ToReadableString<T>(this Expression<T> expression)
        {
            return _translatorRegistry.Translate(expression);
        }
    }
}

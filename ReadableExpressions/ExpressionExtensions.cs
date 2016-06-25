namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        public static string ToReadableString(this Expression expression)
        {
            return _translatorRegistry
                .Translate(expression)?
                .WithoutUnindents();
        }
    }
}

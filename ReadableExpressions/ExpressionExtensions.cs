namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides the Expression translation extension method.
    /// </summary>
    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        /// <summary>
        /// Translates the given <paramref name="expression"/> to source-code string.
        /// </summary>
        /// <param name="expression">The Expression to translate.</param>
        /// <returns>The translated <paramref name="expression"/>.</returns>
        public static string ToReadableString(this Expression expression)
        {
            return _translatorRegistry
                .Translate(expression)?
                .WithoutUnindents();
        }
    }
}

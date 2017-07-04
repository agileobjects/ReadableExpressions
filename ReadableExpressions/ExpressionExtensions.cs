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
		/// <param name="settings">Configuration options for the translation</param>
		/// <returns>The translated <paramref name="expression"/>.</returns>
		public static string ToReadableString(this Expression expression, ReadableStringSettings settings = null)
        {
            return _translatorRegistry
                .Translate(expression, settings ?? new ReadableStringSettings())?
                .WithoutUnindents();
        }
    }
}

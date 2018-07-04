namespace AgileObjects.ReadableExpressions
{
    using System;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
#endif

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
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>.</returns>
        public static string ToReadableString(
            this Expression expression,
            Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            return _translatorRegistry
                .Translate(expression, configuration)?
                .WithoutUnindents();
        }
    }
}

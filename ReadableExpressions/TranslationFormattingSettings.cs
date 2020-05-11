namespace AgileObjects.ReadableExpressions
{
    using Translations.Formatting;

    /// <summary>
    /// Provides formatting-specific configuration options.
    /// </summary>
    public class TranslationFormattingSettings
    {
        internal static readonly TranslationFormattingSettings Default = new TranslationFormattingSettings();

        internal TranslationFormattingSettings()
        {
            Formatter = NullTranslationFormatter.Instance;
        }

        /// <summary>
        /// Format translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format translations.
        /// </param>
        /// <returns>These <see cref="TranslationFormattingSettings"/>, to support a fluent API.</returns>
        public TranslationFormattingSettings FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        internal ITranslationFormatter Formatter { get; private set; }
    }
}
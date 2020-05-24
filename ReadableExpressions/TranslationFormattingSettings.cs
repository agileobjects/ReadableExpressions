namespace AgileObjects.ReadableExpressions
{
    using Translations;
    using Translations.Formatting;

    /// <summary>
    /// Provides formatting-specific configuration options.
    /// </summary>
    public class TranslationFormattingSettings : ITranslationBufferSettings
    {
        internal static readonly TranslationFormattingSettings Default = new TranslationFormattingSettings();

        internal TranslationFormattingSettings()
        {
            Formatter = NullTranslationFormatter.Instance;
            Indent = "    ";
        }

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationFormattingSettings"/>, to support a fluent API.</returns>
        public TranslationFormattingSettings IndentUsing(string indent)
        {
            Indent = indent;
            return this;
        }

        string ITranslationBufferSettings.Indent => Indent;

        internal string Indent { get; private set; }

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

        ITranslationFormatter ITranslationBufferSettings.Formatter => Formatter;

        internal ITranslationFormatter Formatter { get; private set; }
    }
}
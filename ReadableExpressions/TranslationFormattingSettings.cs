namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations;
    using Translations.Formatting;

    /// <summary>
    /// Provides formatting-specific configuration options.
    /// </summary>
    public class TranslationFormattingSettings : ITranslationSettings
    {
        internal static readonly TranslationFormattingSettings Default = new TranslationFormattingSettings();

        private readonly TranslationSettings _wrappedSettings;

        internal TranslationFormattingSettings()
        {
            _wrappedSettings = new TranslationSettings();
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
            _wrappedSettings.IndentUsing(indent);
            return this;
        }

        string ITranslationSettings.Indent => Indent;

        internal string Indent => _wrappedSettings.Indent;

        /// <summary>
        /// Format translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format translations.
        /// </param>
        /// <returns>These <see cref="TranslationFormattingSettings"/>, to support a fluent API.</returns>
        public TranslationFormattingSettings FormatUsing(ITranslationFormatter formatter)
        {
            _wrappedSettings.FormatUsing(formatter);
            return this;
        }

        ITranslationFormatter ITranslationSettings.Formatter => Formatter;

        internal ITranslationFormatter Formatter => _wrappedSettings.Formatter;

        bool ITranslationSettings.FullyQualifyTypeNames => _wrappedSettings.FullyQualifyTypeNames;

        Func<Type, string> ITranslationSettings.AnonymousTypeNameFactory
            => _wrappedSettings.AnonymousTypeNameFactory;
    }
}
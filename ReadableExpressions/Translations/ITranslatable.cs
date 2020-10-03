namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// Implementing classes will translate an Expression to a source-code string.
    /// </summary>
    public interface ITranslatable
    {
        /// <summary>
        /// Gets the number of characters required for the translated Expression.
        /// </summary>
        int TranslationSize { get; }

        /// <summary>
        /// Gets the number of characters required for the translated Expression's formatting markup.
        /// </summary>
        int FormattingSize { get; }

        /// <summary>
        /// Gets the number of characters required for the translated Expression's indentation.
        /// </summary>
        /// <returns>
        /// The number of characters required for the translated Expression's indentation.
        /// </returns>
        int GetIndentSize();

        /// <summary>
        /// Gets the number of lines covered by the translated Expression.
        /// </summary>
        /// <returns>The number of lines covered by the translated Expression.</returns>
        int GetLineCount();

        /// <summary>
        /// Writes the translated Expression to the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="TranslationWriter"/> to which to write the translated Expression.
        /// </param>
        void WriteTo(TranslationWriter writer);
    }
}
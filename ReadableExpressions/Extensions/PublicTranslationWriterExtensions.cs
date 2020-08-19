namespace AgileObjects.ReadableExpressions.Extensions
{
    using Translations;
    using Translations.Formatting;

    /// <summary>
    /// Provides extension methods to use with a <see cref="TranslationWriter"/>.
    /// </summary>
    public static class PublicTranslationWriterExtensions
    {
        /// <summary>
        /// Writes an opening brace to this <paramref name="writer"/>, indenting the contained
        /// translation until <see cref="WriteClosingBraceToTranslation"/> is called.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the opening brace.</param>
        /// <param name="startOnNewLine">
        /// Whether the opening brace should be written on a new line. Defaults to true.
        /// </param>
        public static void WriteOpeningBraceToTranslation(
            this TranslationWriter writer,
            bool startOnNewLine = true)
        {
            if (startOnNewLine && writer.TranslationQuery(q => !q.TranslationEndsWith('{')))
            {
                writer.WriteNewLineToTranslation();
            }

            writer.WriteToTranslation('{');
            writer.WriteNewLineToTranslation();
            writer.Indent();
        }

        /// <summary>
        /// Writes a closing brace to this <paramref name="writer"/> corresponding with an earlier
        /// <see cref="WriteOpeningBraceToTranslation"/> call, unindenting the contained translation.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the closing brace.</param>
        /// <param name="startOnNewLine">
        /// Whether the closing brace should be written on a new line. Defaults to true.
        /// </param>
        public static void WriteClosingBraceToTranslation(
            this TranslationWriter writer,
            bool startOnNewLine = true)
        {
            if (startOnNewLine)
            {
                writer.WriteNewLineToTranslation();
            }

            writer.Unindent();
            writer.WriteToTranslation('}');
        }

        /// <summary>
        /// Writes the given <paramref name="keyword"/> to this <paramref name="writer"/>, with
        /// <see cref="TokenType.Keyword"/> formatting.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="TranslationWriter"/> to which to write the <paramref name="keyword"/>.
        /// </param>
        /// <param name="keyword">The <see cref="TokenType.Keyword"/> to write.</param>
        public static void WriteKeywordToTranslation(this TranslationWriter writer, string keyword)
            => writer.WriteToTranslation(keyword, TokenType.Keyword);

        /// <summary>
        /// Writes the given Type <paramref name="name"/> to this <paramref name="writer"/>, with
        /// <see cref="TokenType.TypeName"/> formatting.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="TranslationWriter"/> to which to write the Type <paramref name="name"/>.
        /// </param>
        /// <param name="name">The <see cref="TokenType.TypeName"/> to write.</param>
        public static void WriteTypeNameToTranslation(this TranslationWriter writer, string name)
            => writer.WriteToTranslation(name, TokenType.TypeName);
    }
}

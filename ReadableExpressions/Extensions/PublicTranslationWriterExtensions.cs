namespace AgileObjects.ReadableExpressions.Extensions;

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

    /// <summary>
    /// Writes a single space character to this <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the space character.</param>
    public static void WriteSpaceToTranslation(this TranslationWriter writer)
        => writer.WriteToTranslation(' ');

    /// <summary>
    /// Writes a single '.' character to this <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the '.' character.</param>
    public static void WriteDotToTranslation(this TranslationWriter writer)
        => writer.WriteToTranslation('.');

    /// <summary>
    /// Writes this <paramref name="translation"/> to a <see cref="TranslationWriter"/> using the
    /// given <paramref name="settings"/>, and returns the translated source-code string.
    /// </summary>
    /// <param name="translation">The <see cref="ITranslation"/> to write.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    /// <returns>The source-code string representation of this <paramref name="translation"/>.</returns>
    public static string WriteUsing(
        this ITranslation translation,
        TranslationSettings settings)
    {
        var writer = new TranslationWriter(settings.Formatter, settings.Indent);
        translation.WriteTo(writer);
        return writer.GetContent();
    }
}
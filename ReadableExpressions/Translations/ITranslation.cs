namespace AgileObjects.ReadableExpressions.Translations;

/// <summary>
/// Implementing classes will translate an Expression to a source-code string.
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// Gets the length of the translated Expression.
    /// </summary>
    int TranslationLength { get; }

    /// <summary>
    /// Writes the translated Expression to the given <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="TranslationWriter"/> to which to write the translated Expression.
    /// </param>
    void WriteTo(TranslationWriter writer);
}
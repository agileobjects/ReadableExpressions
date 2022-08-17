namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// Implementing translation classes may generate no translation output.
    /// </summary>
    public interface IPotentialMultiStatementTranslatable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IPotentialMultiStatementTranslatable"/> will
        /// generate no translation output.
        /// </summary>
        bool IsMultiStatement { get; }
    }
}
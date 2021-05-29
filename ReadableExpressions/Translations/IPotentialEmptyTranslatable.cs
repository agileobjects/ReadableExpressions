namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// Implementing translation classes may generate no translation output.
    /// </summary>
    public interface IPotentialEmptyTranslatable : ITranslatable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IPotentialEmptyTranslatable"/> will
        /// generate no translation output.
        /// </summary>
        bool IsEmpty { get; }
    }
}
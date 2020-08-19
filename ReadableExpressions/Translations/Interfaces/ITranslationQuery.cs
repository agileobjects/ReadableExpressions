namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    /// <summary>
    /// Implementing classes will enable predicates to be defined against an Expression translation.
    /// </summary>
    public interface ITranslationQuery
    {
        /// <summary>
        /// Determines if the current translation ends with the given <paramref name="character"/>.
        /// </summary>
        /// <param name="character">The character for which to make the determination.</param>
        /// <returns>
        /// True if the current translation ends with the given <paramref name="character"/>, otherwise
        /// false.
        /// </returns>
        bool TranslationEndsWith(char character);

        /// <summary>
        /// Determines if the current translation ends with the given <paramref name="substring"/>.
        /// </summary>
        /// <param name="substring">The string for which to make the determination.</param>
        /// <returns>
        /// True if the current translation ends with the given <paramref name="substring"/>, otherwise
        /// false.
        /// </returns>
        bool TranslationEndsWith(string substring);

        /// <summary>
        /// Determines if the current translation ends with an empty line.
        /// </summary>
        /// <returns>True if the current translation ends with an empty line, otherwise false.</returns>
        bool TranslationEndsWithBlankLine();
    }
}
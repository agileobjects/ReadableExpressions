namespace AgileObjects.ReadableExpressions.Translations.Formatting
{
    using System;

    /// <summary>
    /// Implementing classes will add formatting to an Expression translation.
    /// </summary>
    public interface ITranslationFormatter
    {
        /// <summary>
        /// Gets the raw, unformatted version of the given <paramref name="formatted"/> text.
        /// </summary>
        /// <param name="formatted">The formatted text for which to retrieve the raw version.</param>
        /// <returns>
        /// The raw, unformatted version of the given <paramref name="formatted"/> text.
        /// </returns>
        string GetRaw(string formatted);

        /// <summary>
        /// Gets the number of characters required for formatting for the given <paramref name="tokenType"/>.
        /// </summary>
        /// <param name="tokenType">
        /// The <see cref="TokenType"/> for which to retrieve the formatting size.
        /// </param>
        /// <returns>
        /// The number of characters required for formatting for the given <paramref name="tokenType"/>.
        /// </returns>
        int GetFormattingSize(TokenType tokenType);

        /// <summary>
        /// Write the given <paramref name="character"/> using the given <paramref name="characterWriter"/>,
        /// along with any appropriate formatting. This overload is provided to enable encoding of
        /// the value if required.
        /// </summary>
        /// <param name="character">The character to write.</param>
        /// <param name="characterWriter">
        /// An action to execute to write the <paramref name="character"/> to the translation.
        /// </param>
        /// <param name="stringWriter">
        /// An action to execute to write any formatting strings to the translation.
        /// </param>
        /// <param name="type">The <see cref="TokenType"/> of the given <paramref name="character"/>.</param>
        void WriteFormatted(
            char character,
            Action<char> characterWriter,
            Action<string> stringWriter,
            TokenType type);

        /// <summary>
        /// Write the given string <paramref name="value"/> using the given <paramref name="stringWriter"/>,
        /// along with any appropriate formatting. This overload is provided to enable encoding of
        /// the value if required.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="stringWriter">
        /// An action to execute to write the string <paramref name="value"/> to the translation.
        /// </param>
        /// <param name="type">The <see cref="TokenType"/> of the given string <paramref name="value"/>.</param>
        void WriteFormatted(string value, Action<string> stringWriter, TokenType type);

        /// <summary>
        /// Write the given <paramref name="value"/> of the given <paramref name="type"/> using the
        /// given <paramref name="valueWriter"/>, along with any appropriate formatting.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to write.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="valueWriter">
        /// An action to execute to write the <paramref name="value"/> to the translation.
        /// </param>
        /// <param name="stringWriter">
        /// An action to execute to write any formatting strings to the translation.
        /// </param>
        /// <param name="type">The <see cref="TokenType"/> of the given <paramref name="value"/>.</param>
        void WriteFormatted<TValue>(
            TValue value,
            Action<TValue> valueWriter,
            Action<string> stringWriter,
            TokenType type);
    }
}
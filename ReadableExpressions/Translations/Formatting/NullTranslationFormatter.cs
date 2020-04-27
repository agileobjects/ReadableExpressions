namespace AgileObjects.ReadableExpressions.Translations.Formatting
{
    using System;

    internal class NullTranslationFormatter : ITranslationFormatter
    {
        public static readonly ITranslationFormatter Insance = new NullTranslationFormatter();

        public void WriteFormatted(string value, Action<string> stringWriter, TokenType type)
            => stringWriter.Invoke(value);

        public void WriteFormatted(
            char character,
            Action<char> characterWriter,
            Action<string> stringWriter,
            TokenType type)
            => characterWriter.Invoke(character);

        public void WriteFormatted<TValue>(
            TValue value,
            Action<TValue> valueWriter,
            Action<string> stringWriter,
            TokenType type)
            => valueWriter.Invoke(value);
    }
}
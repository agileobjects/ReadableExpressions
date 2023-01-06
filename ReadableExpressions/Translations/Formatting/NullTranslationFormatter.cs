namespace AgileObjects.ReadableExpressions.Translations.Formatting;

using System;

internal class NullTranslationFormatter : ITranslationFormatter
{
    public static readonly ITranslationFormatter Instance = 
        new NullTranslationFormatter();

    public int GetFormattingSize(TokenType tokenType) => 0;

    public void WriteFormatted(
        string value,
        Action<string> stringWriter,
        TokenType type)
    {
        stringWriter.Invoke(value);
    }

    public void WriteFormatted(
        char character,
        Action<char> characterWriter,
        Action<string> stringWriter,
        TokenType type)
    {
        characterWriter.Invoke(character);
    }

    public void WriteFormatted<TValue>(
        TValue value,
        Action<TValue> valueWriter,
        Action<string> stringWriter,
        TokenType type)
    {
        valueWriter.Invoke(value);
    }
}
namespace AgileObjects.ReadableExpressions.Translations;

using Formatting;
using System;
using System.Text;
using static Formatting.TokenType;

/// <summary>
/// Writes a translated Expression into a StringBuilder.
/// </summary>
public class TranslationWriter : ITranslationQuery
{
    private readonly ITranslationFormatter _formatter;
    private readonly string _indent;
    private readonly StringBuilder _content;
    private int _currentIndent;
    private bool _writeIndent;

    internal TranslationWriter(TranslationSettings settings) :
        this(settings.Formatter, settings.Indent)
    {
    }

    internal TranslationWriter(ITranslationFormatter formatter, string indent)
    {
        _formatter = formatter ?? NullTranslationFormatter.Instance;
        _indent = indent;
        _content = new StringBuilder(1_000);
    }

    #region ITranslationQuery

    bool ITranslationQuery.TranslationEndsWith(char character)
    {
        if (_content.Length == 0)
        {
            return false;
        }

        var newlineEncountered = false;

        for (var i = _content.Length; i > 0;)
        {
            --i;
            var contentCharacter = _content[i];

            if (contentCharacter == '\n')
            {
                if (newlineEncountered)
                {
                    return false;
                }

                newlineEncountered = true;
                continue;
            }

            if (char.IsWhiteSpace(contentCharacter))
            {
                if (contentCharacter != character)
                {
                    continue;
                }

                return true;
            }

            return contentCharacter == character;
        }

        return false;
    }

    bool ITranslationQuery.TranslationEndsWith(string substring)
    {
        var substringLength = substring.Length;

        if (_content.Length < substringLength)
        {
            return false;
        }

        var newlineEncountered = false;
        var finalCharacter = substring[substringLength - 1];

        for (var i = _content.Length - 1; i > 0;)
        {
            var contentCharacter = _content[i];

            if (contentCharacter == '\n')
            {
                if (newlineEncountered)
                {
                    return false;
                }

                --i;
                newlineEncountered = true;
                continue;
            }

            if (char.IsWhiteSpace(contentCharacter))
            {
                --i;
                continue;
            }

            if (contentCharacter != finalCharacter)
            {
                return false;
            }

            for (var j = substringLength - 2; j > -1;)
            {
                --i;

                if (_content[i] != substring[j])
                {
                    return false;
                }

                --j;
            }

            return true;
        }

        return false;
    }

    bool ITranslationQuery.TranslationEndsWithBlankLine()
    {
        if (_content.Length < 2)
        {
            return false;
        }

        var newlineEncountered = false;

        for (var i = _content.Length - 1; i > 0;)
        {
            var contentCharacter = _content[i];

            if (contentCharacter == '\n')
            {
                if (newlineEncountered)
                {
                    return true;
                }

                --i;
                newlineEncountered = true;
                continue;
            }

            if (char.IsWhiteSpace(contentCharacter))
            {
                --i;
                continue;
            }

            return false;
        }

        return false;
    }

    #endregion

    /// <summary>
    /// Returns a value indicating whether the given <paramref name="predicate"/> evaluates to
    /// true for the current translation.
    /// </summary>
    /// <param name="predicate">The predicate for which to make the determination.</param>
    /// <returns>
    /// True if the given <paramref name="predicate"/> evaluates to true for the current
    /// translation, otherwise false.
    /// </returns>
    public bool TranslationQuery(Func<ITranslationQuery, bool> predicate)
        => predicate.Invoke(this);

    /// <summary>
    /// Indents the translation writing by the configured or default indent size. Written
    /// translation will continue to be indented until <see cref="Unindent"/> is called.
    /// </summary>
    public void Indent()
    {
        ++_currentIndent;

        if (_writeIndent == false)
        {
            _writeIndent = true;
        }
    }

    /// <summary>
    /// Cancels the previously-requested translation indenting.
    /// </summary>
    public void Unindent() => --_currentIndent;

    /// <summary>
    /// Writes a new line string to the translation.
    /// </summary>
    public void WriteNewLineToTranslation()
    {
        _content.Append(Environment.NewLine);

        if (_currentIndent != 0)
        {
            _writeIndent = true;
        }
    }

    /// <summary>
    /// Writes the given <paramref name="character"/> to the translation.
    /// </summary>
    /// <param name="character">The character to write to the translation.</param>
    public void WriteToTranslation(char character)
        => WriteToTranslation(character, Default);

    private void WriteToTranslation(char character, TokenType tokenType)
    {
        WriteIndentIfRequired();
        _formatter.WriteFormatted(character, Write, Write, tokenType);
    }

    private void Write(char character) => _content.Append(character);

    /// <summary>
    /// Writes the given <paramref name="stringValue"/> to the translation, with the optional
    /// <paramref name="tokenType"/>.
    /// </summary>
    /// <param name="stringValue">The string to write to the translation.</param>
    /// <param name="tokenType">
    /// The <see cref="TokenType"/> indicating the type of code fragment represented by the given
    /// <paramref name="stringValue"/>.
    /// </param>
    public void WriteToTranslation(string stringValue, TokenType tokenType = Default)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            return;
        }

        if (stringValue.Length == 1)
        {
            WriteToTranslation(stringValue[0], tokenType);
            return;
        }

        WriteIndentIfRequired();

        if (tokenType != Default)
        {
            _formatter.WriteFormatted(stringValue, Write, tokenType);
            return;
        }

        Write(stringValue);
    }

    private void Write(string stringValue) => _content.Append(stringValue);

    /// <summary>
    /// Writes the given <paramref name="intValue"/> to the translation.
    /// </summary>
    /// <param name="intValue">The int value to write to the translation.</param>
    public void WriteToTranslation(int intValue)
    {
        WriteIndentIfRequired();
        _formatter.WriteFormatted(intValue, Write, Write, Numeric);
    }

    private void Write(int intValue) => _content.Append(intValue);

    /// <summary>
    /// Writes the given <paramref name="longValue"/> to the translation.
    /// </summary>
    /// <param name="longValue">The long value to write to the translation.</param>
    public void WriteToTranslation(long longValue)
    {
        WriteIndentIfRequired();
        _formatter.WriteFormatted(longValue, Write, Write, Numeric);
    }

    private void Write(long longValue) => _content.Append(longValue);

    /// <summary>
    /// Writes the string representation of the given object <paramref name="value"/> to the
    /// translation.
    /// </summary>
    /// <param name="value">The object value to write to the translation.</param>
    public void WriteToTranslation(object value)
    {
        WriteIndentIfRequired();
        _content.Append(value);
    }

    private void WriteIndentIfRequired()
    {
        if (_writeIndent)
        {
            for (var i = 0; i < _currentIndent; ++i)
            {
                _content.Append(_indent);
            }

            _writeIndent = false;
        }
    }

    /// <summary>
    /// Retrieves the written translation string.
    /// </summary>
    /// <returns>The written translation string</returns>
    public string GetContent()
        => _content.Length > 0 ? _content.ToString() : null;
}
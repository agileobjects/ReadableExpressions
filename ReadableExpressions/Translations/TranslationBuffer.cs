namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if DEBUG
    using System.Diagnostics;
#endif
    using System.Text;
    using Interfaces;
    using static TokenType;

    internal class TranslationBuffer : ITranslationQuery
    {
        private readonly ITranslationFormatter _formatter;
        private readonly StringBuilder _content;
        private int _currentIndent;
        private bool _writeIndent;

        public TranslationBuffer(int estimatedSize)
            : this(NullTranslationFormatter.Insance, estimatedSize)
        {
        }

        public TranslationBuffer(ITranslationFormatter formatter, int estimatedSize)
        {
            _formatter = formatter;
#if DEBUG
            Debug.WriteLine("TranslationBuffer: created with size " + estimatedSize);
#endif
            _content = new StringBuilder(estimatedSize);
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
                    continue;
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

            for (var i = _content.Length; i > 0;)
            {
                var contentCharacter = _content[--i];

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
                    continue;
                }

                if (contentCharacter != finalCharacter)
                {
                    return false;
                }

                for (var j = substringLength - 2; j > -1;)
                {
                    if (_content[--i] != substring[j--])
                    {
                        return false;
                    }
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

            for (var i = _content.Length; i > 0;)
            {
                var contentCharacter = _content[--i];

                if (contentCharacter == '\n')
                {
                    if (newlineEncountered)
                    {
                        return true;
                    }

                    newlineEncountered = true;
                    continue;
                }

                if (char.IsWhiteSpace(contentCharacter))
                {
                    continue;
                }

                return false;
            }

            return false;
        }

        #endregion

        public bool TranslationQuery(Func<ITranslationQuery, bool> predicate)
            => predicate.Invoke(this);

        public void Indent()
        {
            _currentIndent += Constants.Indent.Length;

            if (_writeIndent == false)
            {
                _writeIndent = true;
            }
        }

        public void Unindent()
        {
            _currentIndent -= Constants.Indent.Length;
        }

        public void WriteNewLineToTranslation()
        {
            _content.Append(Environment.NewLine);

            if (_currentIndent != 0)
            {
                _writeIndent = true;
            }
        }

        public void WriteToTranslation(char character, TokenType tokenType = Default)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(character, Write, Write, tokenType);
        }

        private void Write(char character) => _content.Append(character);

        public void WriteToTranslation(string stringValue, TokenType tokenType = Default)
        {
            if (stringValue.Length == 1)
            {
                WriteToTranslation(stringValue[0], tokenType);
                return;
            }

            WriteIndentIfRequired();

            if (tokenType != Default)
            {
                _formatter.WriteFormatted(stringValue, Write, Write, tokenType);
                return;
            }

            Write(stringValue);
        }

        private void Write(string stringValue) => _content.Append(stringValue);

        public void WriteToTranslation(int intValue, TokenType tokenType = Numeric)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(intValue, Write, Write, tokenType);
        }

        private void Write(int intValue) => _content.Append(intValue);

        public void WriteToTranslation(long longValue)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(longValue, Write, Write, Numeric);
        }

        private void Write(long longValue) => _content.Append(longValue);

        public void WriteToTranslation(object value)
        {
            WriteIndentIfRequired();
            _content.Append(value);
        }

        private void WriteIndentIfRequired()
        {
            if (_writeIndent)
            {
                _content.Append(' ', _currentIndent);
                _writeIndent = false;
            }
        }

        public string GetContent()
        {
#if DEBUG
            Debug.WriteLine("TranslationBuffer: final size " + _content.Length);
#endif
            return (_content.Length > 0) ? _content.ToString() : null;
        }
    }

    /// <summary>
    /// Implementing classes will add formatting to an Expression translation.
    /// </summary>
    public interface ITranslationFormatter
    {
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
    }

    internal class NullTranslationFormatter : ITranslationFormatter
    {
        public static readonly ITranslationFormatter Insance = new NullTranslationFormatter();

        public void WriteFormatted<TValue>(
            TValue value,
            Action<TValue> valueWriter,
            Action<string> stringWriter,
            TokenType type)
            => valueWriter.Invoke(value);

        public void WriteFormatted(
            char character,
            Action<char> characterWriter,
            Action<string> stringWriter,
            TokenType type)
            => characterWriter.Invoke(character);
    }
}
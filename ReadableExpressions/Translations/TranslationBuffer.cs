#if DEBUG
using System.Diagnostics;
#endif

namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text;
    using Interfaces;

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

        public void WriteToTranslation(char character)
        {
            WriteIndentIfRequired();

            if (_formatter.Encode(character, out var endoded))
            {
                WriteToTranslation(endoded);
                return;
            }

            _content.Append(character);
        }

        public void WriteToTranslation(
            string stringValue,
            TokenType tokenType = TokenType.Default)
        {
            if (stringValue.Length == 1)
            {
                WriteToTranslation(stringValue[0]);
                return;
            }

            WriteIndentIfRequired();

            WriteIfRequired(_formatter.PreToken(tokenType));

            _content.Append(stringValue);

            WriteIfRequired(_formatter.PostToken(tokenType));
        }

        public void WriteToTranslation(int intValue)
        {
            WriteIndentIfRequired();
            _content.Append(intValue);
        }

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

        private void WriteIfRequired(string value)
        {
            if (value != null)
            {
                _content.Append(value);
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
    /// Defines types of source code elements.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// A default source code element.
        /// </summary>
        Default,

        /// <summary>
        /// A language keyword (new, int, default, etc).
        /// </summary>
        Keyword,

        /// <summary>
        /// A method or constructor parameter.
        /// </summary>
        Parameter,

        /// <summary>
        /// The name of a type.
        /// </summary>
        TypeName,

        /// <summary>
        /// The name of an interface.
        /// </summary>
        InterfaceName,

        /// <summary>
        /// A control statement (if, else, return, etc).
        /// </summary>
        ControlStatement,

        /// <summary>
        /// A constant string or character value.
        /// </summary>
        Text,

        /// <summary>
        /// A constant numeric value.
        /// </summary>
        Numeric,

        /// <summary>
        /// The name of a method.
        /// </summary>
        MethodName
    }

    /// <summary>
    /// Implementing classes will add formatting to an Expression translation.
    /// </summary>
    public interface ITranslationFormatter
    {
        /// <summary>
        /// Gets the formatting string to apply before a token of the given <paramref name="type"/>,
        /// or null if none is necessary.
        /// </summary>
        /// <param name="type">The TokenType for which to retrieve the formatting string.</param>
        /// <returns>
        /// The formatting string to apply before a token of the given <paramref name="type"/>, or
        /// null if none is necessary.
        /// </returns>
        string PreToken(TokenType type);

        /// <summary>
        /// Gets the formatting string to apply after a token of the given <paramref name="type"/>,
        /// or null if none is necessary.
        /// </summary>
        /// <param name="type">The TokenType for which to retrieve the formatting string.</param>
        /// <returns>
        /// The formatting string to apply after a token of the given <paramref name="type"/>, or
        /// null if none is necessary.
        /// </returns>
        string PostToken(TokenType type);

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="character"/> should be replaced
        /// by the output <paramref name="encoded"/> value.
        /// </summary>
        /// <param name="character">The character for which to make the determination.</param>
        /// <param name="encoded">
        /// Populated with the encoded value to write instead of the given <paramref name="character"/>,
        /// if appropriate.
        /// </param>
        /// <returns>
        /// True if the given <paramref name="character"/> should be replaced by the output
        /// <paramref name="encoded"/> value, otherwise false.
        /// </returns>
        bool Encode(char character, out string encoded);
    }

    internal class NullTranslationFormatter : ITranslationFormatter
    {
        public static readonly ITranslationFormatter Insance = new NullTranslationFormatter();

        public string PreToken(TokenType type) => null;

        public string PostToken(TokenType type) => null;

        public bool Encode(char character, out string encoded)
        {
            encoded = null;
            return false;
        }
    }
}
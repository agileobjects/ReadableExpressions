namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text;
    using Interfaces;

    internal class TranslationBuffer : ITranslationQuery
    {

        private readonly StringBuilder _content;
        private int _currentIndent;
        private bool _writeIndent;

        public TranslationBuffer(ITranslatable rootTranslation)
        {
            _content = new StringBuilder(rootTranslation.EstimatedSize);
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
            _content.Append(character);
        }

        public void WriteToTranslation(string stringValue)
        {
            if (stringValue.Length == 1)
            {
                WriteToTranslation(stringValue[0]);
                return;
            }

            WriteIndentIfRequired();
            _content.Append(stringValue);
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

        public string GetContent() => (_content.Length > 0) ? _content.ToString() : null;
    }
}
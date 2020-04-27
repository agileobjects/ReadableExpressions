namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Text.RegularExpressions;
    using Translations.Formatting;
    using static System.Text.RegularExpressions.RegexOptions;

    internal class TranslationHtmlFormatter : ITranslationFormatter
    {
        public static readonly ITranslationFormatter Instance = new TranslationHtmlFormatter();

        private static readonly Regex _htmlMatcher = new Regex("<[^>]+>", Compiled);

        public static string RemoveFormatting(string translation)
        {
            return _htmlMatcher
                .Replace(translation, string.Empty)
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");
        }

        public void WriteFormatted(
            char character,
            Action<char> characterWriter,
            Action<string> stringWriter,
            TokenType type)
        {
            WritePreTokenIfRequired(stringWriter, type);

            if (Encode(character, out var encoded))
            {
                stringWriter.Invoke(encoded);
            }
            else
            {
                characterWriter.Invoke(character);
            }

            WritePostTokenIfRequired(stringWriter, type);
        }

        public void WriteFormatted(string value, Action<string> stringWriter, TokenType type)
        {
            WritePreTokenIfRequired(stringWriter, type);

            stringWriter.Invoke(value.Replace("<", "&lt;").Replace(">", "&gt;"));

            WritePostTokenIfRequired(stringWriter, type);
        }

        public void WriteFormatted<TValue>(
            TValue value,
            Action<TValue> valueWriter,
            Action<string> stringWriter,
            TokenType type)
        {
            WritePreTokenIfRequired(stringWriter, type);

            valueWriter.Invoke(value);

            WritePostTokenIfRequired(stringWriter, type);
        }

        private static string GetPreTokenOrNull(TokenType type)
        {
            switch (type)
            {
                case TokenType.Keyword:
                    return "<span class=\"kw\">";

                case TokenType.Variable:
                    return "<span class=\"vb\">";

                case TokenType.TypeName:
                    return "<span class=\"tn\">";

                case TokenType.InterfaceName:
                    return "<span class=\"in\">";

                case TokenType.ControlStatement:
                    return "<span class=\"cs\">";

                case TokenType.Text:
                    return "<span class=\"tx\">";

                case TokenType.Numeric:
                    return "<span class=\"nm\">";

                case TokenType.MethodName:
                    return "<span class=\"mn\">";

                case TokenType.Comment:
                    return "<span class=\"cm\">";

                default:
                    return null;
            }
        }

        private static string GetPostTokenOrNull(TokenType type)
        {
            switch (type)
            {
                case TokenType.Keyword:
                case TokenType.Variable:
                case TokenType.TypeName:
                case TokenType.InterfaceName:
                case TokenType.ControlStatement:
                case TokenType.Text:
                case TokenType.Numeric:
                case TokenType.MethodName:
                case TokenType.Comment:
                    return "</span>";

                default:
                    return null;
            }
        }

        private static void WritePreTokenIfRequired(Action<string> stringWriter, TokenType type)
            => WriteIfRequired(GetPreTokenOrNull(type), stringWriter);

        private static void WritePostTokenIfRequired(Action<string> stringWriter, TokenType type)
            => WriteIfRequired(GetPostTokenOrNull(type), stringWriter);

        private static void WriteIfRequired(string value, Action<string> stringWriter)
        {
            if (value != null)
            {
                stringWriter.Invoke(value);
            }
        }

        private static bool Encode(char character, out string encoded)
        {
            switch (character)
            {
                case '<':
                    encoded = "&lt;";
                    return true;

                case '>':
                    encoded = "&gt;";
                    return true;

                default:
                    encoded = null;
                    return false;
            }
        }
    }
}
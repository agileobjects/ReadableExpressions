namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Linq;

    internal static class StringExtensions
    {
        private static readonly char[] _terminatingCharacters = { ';', '}', ':', ',' };

        public static bool IsTerminated(this string codeLine)
        {
            return _terminatingCharacters.Contains(codeLine[codeLine.Length - 1]) ||
                CommentExpression.IsComment(codeLine);
        }

        private static readonly string[] _newLines = { Environment.NewLine };

        public static string[] SplitToLines(this string line)
        {
            return line.Split(_newLines, StringSplitOptions.None);
        }

        private const string Indent = "    ";

        public static string Indented(this string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            if (line.Contains(Environment.NewLine))
            {
                return string.Join(
                    Environment.NewLine,
                    line.SplitToLines().Select(l => l.Indented()));
            }

            return Indent + line;
        }

        private const string UnindentPlaceholder = "*unindent*";

        public static string Unindented(this string line)
        {
            return UnindentPlaceholder + line;
        }

        public static string WithoutUnindents(this string code)
        {
            if (code.Contains(UnindentPlaceholder))
            {
                return code
                    .Replace(Indent + UnindentPlaceholder, null)
                    .Replace(UnindentPlaceholder, null);
            }

            return code.Replace(UnindentPlaceholder, null);
        }
    }
}
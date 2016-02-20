namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Linq;

    internal static class StringExtensions
    {
        private static readonly char[] _terminatingCharacters = { ';', '}', ':' };

        public static bool IsTerminated(this string codeLine)
        {
            return _terminatingCharacters.Contains(codeLine[codeLine.Length - 1]);
        }

        private const string UnindentPlaceholder = "*unindent*";

        public static string Unindented(this string line)
        {
            return UnindentPlaceholder + line;
        }

        public static string WithoutUnindents(this string code)
        {
            return code.Replace(UnindentPlaceholder, null);
        }

        public static bool IsUnindented(this string line)
        {
            return line.StartsWith(UnindentPlaceholder, StringComparison.Ordinal);
        }

        public static string WithoutUnindent(this string line)
        {
            return line.Substring(UnindentPlaceholder.Length);
        }
    }
}
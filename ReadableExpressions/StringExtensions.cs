namespace AgileObjects.ReadableExpressions
{
    using System.Linq;

    internal static class StringExtensions
    {
        private static readonly char[] _terminatingCharacters = { ';', '}', ':' };

        public static bool IsTerminated(this string codeLine)
        {
            return _terminatingCharacters.Contains(codeLine[codeLine.Length - 1]);
        }

        private const string UnindentPlaceholder = "*unindent*";
        private const string Indent = "    ";

        public static string Indented(this string line)
        {
            return Indent + line;
        }

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
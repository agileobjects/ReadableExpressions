namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;

    internal static class StringExtensions
    {
        private static readonly char[] _terminatingCharacters = { ';', '}', ':' };

        public static bool IsTerminated(this string codeLine)
        {
            return _terminatingCharacters.Contains(codeLine[codeLine.Length - 1]);
        }
    }
}
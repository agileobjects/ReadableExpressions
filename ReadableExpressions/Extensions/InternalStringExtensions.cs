namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
#if NET35
    using System.Linq;
#endif
    using static System.StringSplitOptions;

    internal static class InternalStringExtensions
    {
        private static readonly string[] _newLines = { Environment.NewLine };

        public static int GetLineCount(this string value) => value.SplitToLines().Length;

        public static string[] SplitToLines(this string value) 
            => value?.Split(_newLines, None) ?? Enumerable<string>.EmptyArray;

        public static string ToPascalCase(this string value)
            => char.ToUpperInvariant(value[0]) + value.Substring(1);

        public static string ToCamelCase(this string value)
            => char.ToLowerInvariant(value[0]) + value.Substring(1);

        public static string Join(this IEnumerable<string> items, string separator)
        {
#if NET35
            return string.Join(separator, items.ToArray());
#else
            return string.Join(separator, items);
#endif
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
#if NET35
            return (value == null) || (value.Trim() == string.Empty);
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }

        public static bool StartsWith(this string value, char character)
        {
            return (value.Length > 0) && (value[0] == character);
        }

        private static bool EndsWith(this string value, char character)
        {
            return (value != string.Empty) && value.EndsWithNoEmptyCheck(character);
        }

        private static bool EndsWithNoEmptyCheck(this string value, char character)
        {
            return value[value.Length - 1] == character;
        }

        public static bool IsComment(this string codeLine)
        {
            if (codeLine == null)
            {
                return false;
            }

            for (int i = 0, l = codeLine.Length; i < l; ++i)
            {
                var character = codeLine[i];

                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                if ((l - i) < 3)
                {
                    return false;
                }

                return character == '/' && codeLine[i + 1] == '/' && codeLine[i + 2] == ' ';
            }

            return false;
        }
    }
}
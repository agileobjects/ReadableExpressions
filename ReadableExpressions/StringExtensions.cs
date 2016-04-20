namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class StringExtensions
    {
        private static readonly char[] _terminatingCharacters = { ';', '}', ':', ',' };

        public static bool IsTerminated(this string codeLine)
        {
            return codeLine.EndsWithAny(_terminatingCharacters) ||
                CommentExpression.IsComment(codeLine);
        }

        public static string Unterminated(this string codeLine)
        {
            return codeLine.EndsWith(';')
                ? codeLine.Substring(0, codeLine.Length - 1)
                : codeLine;
        }

        private static readonly string[] _newLines = { Environment.NewLine };

        public static string[] SplitToLines(this string line)
        {
            return line.Split(_newLines, StringSplitOptions.None);
        }

        private const string IndentSpaces = "    ";

        public static string Indent(this string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            if (line.Contains(Environment.NewLine))
            {
                return string.Join(
                    Environment.NewLine,
                    line.SplitToLines().Select(l => l.Indent()));
            }

            return IndentSpaces + line;
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
                    .Replace(IndentSpaces + UnindentPlaceholder, null)
                    .Replace(UnindentPlaceholder, null);
            }

            return code;
        }

        public static string WithoutSurroundingParentheses(this string value, Expression expression)
        {
            if (string.IsNullOrEmpty(value) || KeepSurroundingParentheses(expression))
            {
                return value;
            }

            if (value.StartsWith('(') && value.EndsWith(')'))
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        private static bool KeepSurroundingParentheses(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Conditional:
                    return true;

                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Object?.NodeType == ExpressionType.Convert;
            }

            return false;
        }

        public static bool StartsWithNewLine(this string value)
        {
            return value.StartsWith(Environment.NewLine, StringComparison.Ordinal);
        }

        public static bool StartsWith(this string value, char character)
        {
            return value[0] == character;
        }

        public static bool EndsWith(this string value, char character)
        {
            return value[value.Length - 1] == character;
        }

        private static bool EndsWithAny(this string value, IEnumerable<char> characters)
        {
            return characters.Any(value.EndsWith);
        }
    }
}
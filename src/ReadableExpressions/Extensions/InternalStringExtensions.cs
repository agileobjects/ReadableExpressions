namespace AgileObjects.ReadableExpressions.Extensions;

using System;
using static System.StringSplitOptions;

internal static class InternalStringExtensions
{
    private static readonly string[] _newLines = [Environment.NewLine];

    public static string[] SplitToLines(this string value) => 
        value?.Split(_newLines, None) ?? Enumerable<string>.EmptyArray;

    public static string ToPascalCase(this string value) => 
        char.ToUpperInvariant(value[0]) + value.Substring(1);

    public static string ToCamelCase(this string value) => 
        char.ToLowerInvariant(value[0]) + value.Substring(1);

    public static bool IsNullOrWhiteSpace(this string value)
    {
#if NET35
        return value == null || value.Trim() == string.Empty;
#else
        return string.IsNullOrWhiteSpace(value);
#endif
    }

    public static bool StartsWith(this string value, char character) => 
        value.Length > 0 && value[0] == character;
}
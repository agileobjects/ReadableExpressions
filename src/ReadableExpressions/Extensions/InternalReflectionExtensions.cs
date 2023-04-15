namespace AgileObjects.ReadableExpressions.Extensions
{
#if FEATURE_VALUE_TUPLE
    using System;
#endif
    using System.Collections.Generic;
    using System.Reflection;
    using NetStandardPolyfills;
    using Translations.Reflection;
#if FEATURE_VALUE_TUPLE
    using static System.StringComparison;
#endif

    internal static class InternalReflectionExtensions
    {
        private static readonly Dictionary<string, string> _typeNameKeywords = new()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            { typeof(string).FullName, "string" },
            { typeof(int).FullName, "int" },
            { typeof(bool).FullName, "bool" },
            { typeof(decimal).FullName, "decimal" },
            { typeof(long).FullName, "long" },
            { typeof(double).FullName, "double" },
            { typeof(object).FullName, "object" },
            { typeof(byte).FullName, "byte" },
            { typeof(short).FullName, "short" },
            { typeof(float).FullName, "float" },
            { typeof(char).FullName, "char" },
            { typeof(uint).FullName, "uint" },
            { typeof(ulong).FullName, "ulong" },
            { typeof(sbyte).FullName, "sbyte" },
            { typeof(ushort).FullName, "ushort" },
            { typeof(void).FullName, "void" }
            // ReSharper restore AssignNullToNotNullAttribute
        };

        public static ICollection<string> TypeNames => _typeNameKeywords.Values;

        public static string GetKeywordOrNull(this IType type)
        {
            if (type.FullName == null)
            {
                return null;
            }

            return _typeNameKeywords.TryGetValue(type.FullName, out var substitutedName)
                ? substitutedName : null;
        }

        public static object GetValue(this MemberInfo member, object subject)
        {
            var hasSubject = subject != null;

            return member switch
            {
                FieldInfo field when hasSubject || field.IsStatic => field.GetValue(subject),
                PropertyInfo property when hasSubject || property.IsStatic() => property.GetValue(subject,
                    Enumerable<object>.EmptyArray),
                _ => null
            };
        }

#if FEATURE_VALUE_TUPLE
        public static bool IsValueTuple(this Type type)
        {
            return type.IsGenericType() &&
                   type.FullName?.StartsWith("System.ValueTuple`", Ordinal) == true;
        }
#endif
    }
}

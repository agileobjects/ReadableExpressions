using System.Linq;

namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;

    internal static class InternalTypeExtensions
    {
        private static readonly Dictionary<Type, string> _typeNameSubstitutions = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(bool), "bool" },
            { typeof(decimal), "decimal" },
            { typeof(long), "long" },
            { typeof(double), "double" },
            { typeof(object), "object" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(float), "float" },
            { typeof(char), "char" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(sbyte), "sbyte" },
            { typeof(ushort), "ushort" },
        };

        internal static ICollection<string> TypeNames => _typeNameSubstitutions.Values;

        public static string GetSubstitutionOrNull(this Type type)
        {
            return _typeNameSubstitutions.TryGetValue(type, out var substitutedName)
                ? substitutedName : null;
        }

        public static string GetSubstitutionOrNull(this string typeFullName)
        {
            var matchingType = _typeNameSubstitutions
                .FirstOrDefault(kvp => kvp.Key.FullName == typeFullName);

            return matchingType.Value;
        }
    }
}

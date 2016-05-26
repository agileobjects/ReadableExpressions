namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<string, string> _typeNameSubstitutions = new Dictionary<string, string>
        {
            { typeof(byte).FullName, "byte" },
            { typeof(sbyte).FullName, "sbyte" },
            { typeof(short).FullName, "short" },
            { typeof(ushort).FullName, "ushort" },
            { typeof(int).FullName, "int" },
            { typeof(uint).FullName, "uint" },
            { typeof(long).FullName, "long" },
            { typeof(ulong).FullName, "ulong" },
            { typeof(float).FullName, "float" },
            { typeof(decimal).FullName, "decimal" },
            { typeof(double).FullName, "double" },
            { typeof(string).FullName, "string" },
            { typeof(object).FullName, "object" },
        };

        internal static IEnumerable<string> TypeNames => _typeNameSubstitutions.Values;

        public static string GetFriendlyName(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }

            if (!type.IsGenericType)
            {
                return type.FullName.GetSubstitutionOrNull() ?? type.Name;
            }

            var typeGenericTypeArguments = type.GetGenericArguments();

            var typeGenericTypeArgumentFriendlyNames =
                string.Join(", ", typeGenericTypeArguments.Select(ga => ga.GetFriendlyName()));

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return typeGenericTypeArgumentFriendlyNames + "?";
            }

            var typeGenericNameWithAngleBrackets =
                type.Name.Insert(type.Name.IndexOf("`", StringComparison.Ordinal), "<") + ">";

            return typeGenericNameWithAngleBrackets.Replace(
                "`" + typeGenericTypeArguments.Length,
                typeGenericTypeArgumentFriendlyNames);
        }

        public static string GetSubstitutionOrNull(this string typeFullName)
        {
            string substitutedName;

            return _typeNameSubstitutions.TryGetValue(typeFullName, out substitutedName)
                ? substitutedName : null;
        }

        public static bool CanBeNull(this Type type)
        {
            return type.IsClass || type.IsInterface || type.IsNullableType();
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}

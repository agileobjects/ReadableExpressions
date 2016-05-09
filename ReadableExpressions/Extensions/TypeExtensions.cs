namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _typeNameSubstitutions = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(string), "string" }
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
                string substitutedName;

                return _typeNameSubstitutions.TryGetValue(type, out substitutedName)
                    ? substitutedName : type.Name;
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
    }
}

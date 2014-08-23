namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class TypeExtensions
    {
        private static readonly Dictionary<string, string> _typeNameSubstitutions = new Dictionary<string, string>
        {
            { "Int64", "Long" }
        };

        public static string GetFriendlyName(this Type type)
        {
            if (!type.IsGenericType)
            {
                string substitutedName;

                return _typeNameSubstitutions.TryGetValue(type.Name, out substitutedName)
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
                "`" + typeGenericTypeArguments.Count(),
                typeGenericTypeArgumentFriendlyNames);
        }
    }
}

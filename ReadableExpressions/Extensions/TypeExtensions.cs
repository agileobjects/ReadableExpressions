namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class TypeExtensions
    {
        private static readonly Dictionary<string, string> _typeNameSubstitutions = new Dictionary<string, string>
        {
            { typeof(byte).FullName, "byte" },
            { typeof(sbyte).FullName, "sbyte" },
            { typeof(char).FullName, "char" },
            { typeof(bool).FullName, "bool" },
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

            Type underlyingNullableType;

            if ((underlyingNullableType = Nullable.GetUnderlyingType(type)) != null)
            {
                return underlyingNullableType.GetFriendlyName() + "?";
            }

            return GetGenericTypeName(type);
        }

        public static string GetSubstitutionOrNull(this string typeFullName)
        {
            string substitutedName;

            return _typeNameSubstitutions.TryGetValue(typeFullName, out substitutedName)
                ? substitutedName : null;
        }

        private static string GetGenericTypeName(Type genericType)
        {
            var typeGenericTypeArguments = genericType.GetGenericArguments();
            var genericTypeName = genericType.Name;

            // ReSharper disable once PossibleNullReferenceException
            while (genericType.IsNested)
            {
                genericType = genericType.DeclaringType;
                genericTypeName = genericType.Name + "." + genericTypeName;
            }

            var typeGenericTypeArgumentFriendlyNames =
                string.Join(", ", typeGenericTypeArguments.Select(ga => ga.GetFriendlyName()));

            return genericTypeName.Replace(
                "`" + typeGenericTypeArguments.Length,
                "<" + typeGenericTypeArgumentFriendlyNames + ">");
        }

        public static bool CanBeNull(this Type type)
        {
            return type.IsClass || type.IsInterface || type.IsNullableType();
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        // See http://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        public static bool IsAnonymous(this Type type)
        {
            return type.IsGenericType &&
                   type.Name.Contains("AnonymousType") &&
                   (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$")) &&
                   (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic &&
                   Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), inherit: false);
        }
    }
}

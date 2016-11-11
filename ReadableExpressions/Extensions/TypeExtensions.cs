namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif

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

        public static string GetSubstitutionOrNull(this string typeFullName)
        {
            string substitutedName;

            return _typeNameSubstitutions.TryGetValue(typeFullName, out substitutedName)
                ? substitutedName : null;
        }
    }
}

namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using NetStandardPolyfills;
    using Translations.Reflection;

    internal static class InternalReflectionExtensions
    {
        private static readonly Dictionary<string, string> _typeNameSubstitutions =
            new Dictionary<string, string>
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

        public static ICollection<string> TypeNames => _typeNameSubstitutions.Values;

        public static string GetSubstitutionOrNull(this IType type)
        {
            if (type.FullName == null)
            {
                return null;
            }

            return _typeNameSubstitutions.TryGetValue(type.FullName, out var substitutedName)
                ? substitutedName : null;
        }

        public static bool IsPropertyGetterOrSetterCall(this MethodInfo method, out PropertyInfo property)
        {
            if (method.IsAbstract || method.HasAttribute<CompilerGeneratedAttribute>())
            {
                // Find declaring property
                property = GetPropertyOrNull(method);

                if (property != null)
                {
                    return true;
                }
            }

            property = null;
            return false;
        }

        private static PropertyInfo GetPropertyOrNull(MethodInfo method)
        {
            var hasSingleArgument = method.GetParameters().Length == 1;
            var hasReturnType = method.ReturnType != typeof(void);

            if (hasSingleArgument == hasReturnType)
            {
                return null;
            }

            var type = method.DeclaringType;

            var allProperties =
                type.GetPublicInstanceProperties()
                    .Concat(type.GetNonPublicInstanceProperties())
                    .Concat(type.GetPublicStaticProperties())
                    .Concat(type.GetNonPublicStaticProperties());

            return allProperties.FirstOrDefault(property => Equals(
                hasReturnType
                    ? property.GetGetter(nonPublic: true)
                    : property.GetSetter(nonPublic: true),
                method));
        }

        public static string GetAssemblyLocation(this Type type)
        {
            var assembly = type.GetAssembly();

#if NETSTANDARD
            return assembly.GetType()
                .GetPublicInstanceProperty("Location")?
                .GetValue(assembly) as string;
#else
            return assembly.Location;
#endif
        }
    }
}

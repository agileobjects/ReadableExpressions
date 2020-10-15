namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using NetStandardPolyfills;

    internal static class InternalReflectionExtensions
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
            { typeof(void), "void" }
        };

        internal static ICollection<string> TypeNames => _typeNameSubstitutions.Values;

        public static string GetSubstitutionOrNull(this Type type)
        {
            return _typeNameSubstitutions.TryGetValue(type, out var substitutedName)
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

        public static bool IsGenericParameter(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().IsGenericParameter;
#else
            return type.IsGenericParameter;
#endif
        }

        public static GenericParameterAttributes GetConstraints(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().GenericParameterAttributes;
#else
            return type.GenericParameterAttributes;
#endif
        }

        public static IList<Type> GetConstraintTypes(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().GetGenericParameterConstraints();
#else
            return type.GetGenericParameterConstraints();
#endif
        }
    }
}

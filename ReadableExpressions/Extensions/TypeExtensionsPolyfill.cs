namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensionsPolyfill
    {
        public static bool IsClass(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static Type GetBaseType(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static TypeAttributes GetAttributes(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().Attributes;
#else
            return type.Attributes;
#endif
        }

        public static bool HasAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
#if NET_STANDARD
            return type.GetTypeInfo().GetCustomAttribute<TAttribute>(inherit: false) != null;
#else
            return Attribute.IsDefined(type, typeof(TAttribute), inherit: false);
#endif
        }

        public static IEnumerable<MethodInfo> GetNonPublicInstanceMethods(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMethods.Where(m => !m.IsPublic && !m.IsStatic);
#else
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        }

#if NET_STANDARD
        private static readonly Dictionary<Type, NetStandardTypeCode> _typeCodesByType = new Dictionary<Type, NetStandardTypeCode>
        {
            {typeof(bool), NetStandardTypeCode.Boolean },
            {typeof(byte), NetStandardTypeCode.Byte },
            {typeof(char), NetStandardTypeCode.Char},
            {typeof(DateTime), NetStandardTypeCode.DateTime},
            {typeof(decimal), NetStandardTypeCode.Decimal},
            {typeof(double), NetStandardTypeCode.Double },
            {typeof(short), NetStandardTypeCode.Int16 },
            {typeof(int), NetStandardTypeCode.Int32 },
            {typeof(long), NetStandardTypeCode.Int64 },
            {typeof(object), NetStandardTypeCode.Object},
            {typeof(sbyte), NetStandardTypeCode.SByte },
            {typeof(float), NetStandardTypeCode.Single },
            {typeof(string), NetStandardTypeCode.String },
            {typeof(ushort), NetStandardTypeCode.UInt16 },
            {typeof(uint), NetStandardTypeCode.UInt32 },
            {typeof(ulong), NetStandardTypeCode.UInt64 },
        };
#endif
        public static NetStandardTypeCode GetTypeCode(this Type type)
        {
#if NET_STANDARD
            if (type == null)
            {
                return NetStandardTypeCode.Empty;
            }

            if (type.FullName == "System.DBNull")
            {
                return NetStandardTypeCode.DBNull;
            }

            if (type.IsEnum())
            {
                type = Enum.GetUnderlyingType(type);
            }

            NetStandardTypeCode typeCode;

            if (_typeCodesByType.TryGetValue(type, out typeCode))
            {
                return typeCode;
            }

            return NetStandardTypeCode.Object;
#else
            return (NetStandardTypeCode)Type.GetTypeCode(type);
#endif
        }
    }
}

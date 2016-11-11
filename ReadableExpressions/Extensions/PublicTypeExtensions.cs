namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET_STANDARD
    using System.Reflection;
#endif
    using NetStandardPolyfills;

    /// <summary>
    /// Provides a set of static extension methods for type information.
    /// </summary>
    public static class PublicTypeExtensions
    {
        /// <summary>
        /// Returns a friendly, readable version of the name of the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which to retrieve a friendly, readable name.</param>
        /// <returns>A friendly, readable version of the name of the given <paramref name="type"/>.</returns>
        public static string GetFriendlyName(this Type type)
        {
            if (type.FullName == null)
            {
                // An open generic parameter Type:
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }

            if (!type.IsGenericType())
            {
                var qualifiedTypeName = type.FullName.GetSubstitutionOrNull() ?? type.Name;

                if (type.IsNested)
                {
                    return type.DeclaringType.GetFriendlyName() + "." + qualifiedTypeName;
                }

                return qualifiedTypeName;
            }

            Type underlyingNullableType;

            if ((underlyingNullableType = Nullable.GetUnderlyingType(type)) != null)
            {
                return underlyingNullableType.GetFriendlyName() + "?";
            }

            return GetGenericTypeName(type);
        }

        private static string GetGenericTypeName(Type genericType)
        {
            var typeGenericTypeArguments = genericType.GetGenericArguments();

            if (!genericType.IsNested)
            {
                return GetGenericTypeName(genericType.Name, typeGenericTypeArguments.Length, typeGenericTypeArguments);
            }

            var genericTypeName = genericType.Name;

            // ReSharper disable once PossibleNullReferenceException
            while (genericType.IsNested)
            {
                genericType = genericType.DeclaringType;
                var parentTypeName = genericType.Name;

                var backtickIndex = parentTypeName.IndexOf("`", StringComparison.Ordinal);

                if (backtickIndex != -1)
                {
                    var numberOfParameters = int.Parse(parentTypeName.Substring(backtickIndex + 1));

                    Type[] typeArguments;

                    if (numberOfParameters == typeGenericTypeArguments.Length)
                    {
                        typeArguments = typeGenericTypeArguments;
                    }
                    else
                    {
                        typeArguments = new Type[numberOfParameters];
                        var numberOfRemainingTypeArguments = typeGenericTypeArguments.Length - numberOfParameters;
                        var typeGenericTypeArgumentsSubset = new Type[numberOfRemainingTypeArguments];

                        Array.Copy(
                            typeGenericTypeArguments,
                            numberOfRemainingTypeArguments,
                            typeArguments,
                            0,
                            numberOfParameters);

                        Array.Copy(
                            typeGenericTypeArguments,
                            0,
                            typeGenericTypeArgumentsSubset,
                            0,
                            numberOfRemainingTypeArguments);

                        typeGenericTypeArguments = typeGenericTypeArgumentsSubset;
                    }

                    parentTypeName = GetGenericTypeName(parentTypeName, numberOfParameters, typeArguments);
                }

                genericTypeName = parentTypeName + "." + genericTypeName;
            }

            return genericTypeName;
        }

        private static string GetGenericTypeName(string typeName, int numberOfParameters, IEnumerable<Type> typeArguments)
        {
            var typeGenericTypeArgumentFriendlyNames =
                string.Join(", ", typeArguments.Select(tp => GetFriendlyName(tp)));

            typeName = typeName.Replace(
                "`" + numberOfParameters,
                "<" + typeGenericTypeArgumentFriendlyNames + ">");

            return typeName;
        }

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> can be null.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> can be null, otherwise false.</returns>
        public static bool CanBeNull(this Type type)
        {
            return type.IsClass() || type.IsInterface() || type.IsNullableType();
        }

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> is a Nullable&lt;T&gt;.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> is a Nullable&lt;T&gt;, otherwise false.</returns>
        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
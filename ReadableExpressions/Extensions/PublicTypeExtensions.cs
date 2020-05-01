namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides a set of static extension methods for type information.
    /// </summary>
    public static partial class PublicTypeExtensions
    {
        /// <summary>
        /// Determines if this <paramref name="type"/> is an enumerable Type.
        /// </summary>
        /// <param name="type">The Type for which to make the determination.</param>
        /// <returns>True if this <paramref name="type"/> is an enumerable Type, otherwise false.</returns>
        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray ||
                  (type != typeof(string) &&
                   type.IsAssignableTo(typeof(IEnumerable)));
        }

        /// <summary>
        /// Determines if this <paramref name="type"/> is a Dictionary Type.
        /// </summary>
        /// <param name="type">The Type for which to make the determination.</param>
        /// <returns>True if this <paramref name="type"/> is a Dictionary Type, otherwise false.</returns>
        public static bool IsDictionary(this Type type)
            => !GetDictionaryTypes(type).Equals(default(KeyValuePair<Type, Type>));

        /// <summary>
        /// Gets a KeyValuePair containing the key and value Types of this Dictionary <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the key and value Types.</param>
        /// <returns>A KeyValuePair containing the key and value Types of this Dictionary <paramref name="type"/>.</returns>
        public static KeyValuePair<Type, Type> GetDictionaryTypes(this Type type)
        {
            var dictionaryType = GetDictionaryType(type);

            return (dictionaryType != null)
                ? GetDictionaryTypesFrom(dictionaryType)
                : default(KeyValuePair<Type, Type>);
        }

        /// <summary>
        /// Gets the Dictionary Type of this <paramref name="type"/> - either the Dictionary Type it is, or
        /// the first IDictionary Type it implements. If this <paramref name="type"/> is not a Dictionary
        /// Type, returns null.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the Dictionary Type.</param>
        /// <returns>The Dictionary Type of this <paramref name="type"/>, or null if there is none.</returns>
        public static Type GetDictionaryType(this Type type)
        {
            if (type.IsGenericType())
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                if ((typeDefinition == typeof(Dictionary<,>)) || (typeDefinition == typeof(IDictionary<,>)))
                {
                    return type;
                }
            }

            var interfaceType = InternalEnumerableExtensions.FirstOrDefault(type
                    .GetAllInterfaces(), t => t.IsClosedTypeOf(typeof(IDictionary<,>)));

            return interfaceType;
        }

        private static KeyValuePair<Type, Type> GetDictionaryTypesFrom(Type type)
        {
            var types = type.GetGenericTypeArguments();
            return new KeyValuePair<Type, Type>(types[0], types[1]);
        }

        /// <summary>
        /// Gets the element Type for this <paramref name="enumerableType"/>.
        /// </summary>
        /// <param name="enumerableType">The enumerable Type for which to retrieve the element Type.</param>
        /// <returns>
        /// The element Type for this <paramref name="enumerableType"/>, or null if this Type is not enumerable.
        /// </returns>
        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            if (enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }

            if (enumerableType.IsGenericType())
            {
                return enumerableType.GetGenericTypeArguments().Last();
            }

            var enumerableInterfaceType = enumerableType
                .GetAllInterfaces()
                .FirstOrDefault(interfaceType => interfaceType.IsClosedTypeOf(typeof(IEnumerable<>)));

            return enumerableInterfaceType?.GetGenericTypeArguments().First() ?? typeof(object);
        }

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> can be null.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> can be null, otherwise false.</returns>
        public static bool CanBeNull(this Type type)
            => type.IsClass() || type.IsInterface() || type.IsNullableType();

        /// <summary>
        /// Returns a value indicating if the given <paramref name="type"/> is a Nullable{T}.
        /// </summary>
        /// <param name="type">The type for which to make the determination.</param>
        /// <returns>True if the given <paramref name="type"/> is a Nullable{T}, otherwise false.</returns>
        public static bool IsNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// Gets the underlying non-nullable Type of this <paramref name="type"/>, or returns this
        /// <paramref name="type"/> if it is not nullable.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the underlying non-nullable Type.</param>
        /// <returns>
        /// The underlying non-nullable Type of this <paramref name="type"/>, or returns this
        /// <paramref name="type"/> if it is not nullable.
        /// </returns>
        [DebuggerStepThrough]
        public static Type GetNonNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
    }
}
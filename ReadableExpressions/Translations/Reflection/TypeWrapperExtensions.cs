namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Linq;
    using static BclTypeWrapper;

    /// <summary>
    /// Provides extension methods for <see cref="IType"/> instances.
    /// </summary>
    public static class TypeWrapperExtensions
    {
        /// <summary>
        /// Determines if a value of this <see cref="IType"/> is assignable to a variable of the
        /// given <paramref name="otherType"/>.
        /// </summary>
        /// <param name="type">The assignable <see cref="IType"/> for which to make the determination.</param>
        /// <param name="otherType">The assigned <see cref="IType"/> for which to make the determination.</param>
        /// <returns>
        /// True if a value of this <see cref="IType"/> is assignable to a variable of the given
        /// <paramref name="otherType"/>, otherwise false.
        /// </returns>
        public static bool IsAssignableTo(this IType type, IType otherType)
            => AreAssignable(type, otherType);

        private static bool AreAssignable(IType type, IType otherType)
        {
            if (ReferenceEquals(otherType, Object) || type.Equals(otherType))
            {
                return true;
            }

            if (type.IsClass)
            {
                var baseType = type.BaseType;

                while (!ReferenceEquals(baseType, Object))
                {
                    if (baseType.Equals(otherType))
                    {
                        return true;
                    }

                    baseType = baseType.BaseType;
                }
            }

            return type.AllInterfaces.Any(itf => itf.Equals(otherType));
        }
    }
}
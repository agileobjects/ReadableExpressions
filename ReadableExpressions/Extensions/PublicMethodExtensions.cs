namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetStandardPolyfills;
    using Translations.Reflection;

    /// <summary>
    /// Provides extension methods to use with <see cref="IMethod"/> implementations.
    /// </summary>
    public static class PublicMethodExtensions
    {
        /// <summary>
        /// Gets the Types of this <see cref="IMethod"/>'s generic arguments, if they are not all
        /// implicitly specified by its arguments.
        /// </summary>
        /// <param name="method">The <see cref="IMethod"/> for which to retrieve the argument Types.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>
        /// The Types of this <see cref="IMethod"/>'s generic arguments if they are not all implicitly
        /// specified by its arguments, otherwise an empty Type array.
        /// </returns>
        public static IList<Type> GetRequiredExplicitGenericArguments(
            this IMethod method,
            TranslationSettings settings)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<Type>.EmptyArray;
            }

            var methodGenericDefinition = method.GetGenericMethodDefinition();
            var requiredGenericParameterTypes = methodGenericDefinition.GetGenericArguments().ToList();

            if (settings.UseImplicitGenericParameters)
            {
                RemoveSuppliedGenericTypeParameters(
                    methodGenericDefinition.GetParameters().Project(p => p.Type),
                    requiredGenericParameterTypes);
            }

            return InternalEnumerableExtensions.Any(requiredGenericParameterTypes)
                ? method.GetGenericArguments()
                : Enumerable<Type>.EmptyArray;
        }

        private static void RemoveSuppliedGenericTypeParameters(
            IEnumerable<Type> types,
            ICollection<Type> genericParameterTypes)
        {
            foreach (var type in types.Project(t => t.IsByRef ? t.GetElementType() : t))
            {
                if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                {
                    genericParameterTypes.Remove(type);
                }

                if (type.IsGenericType())
                {
                    RemoveSuppliedGenericTypeParameters(type.GetGenericTypeArguments(), genericParameterTypes);
                }
            }
        }
    }
}
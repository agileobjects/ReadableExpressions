namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NetStandardPolyfills;
    using Translations.Reflection;

    /// <summary>
    /// Provides extension methods to use with <see cref="IMethod"/> implementations.
    /// </summary>
    public static class PublicMethodExtensions
    {
        /// <summary>
        /// Gets the Types of this <paramref name="method"/>'s <see cref="IGenericArgument"/>s, if
        /// they are not all implicitly specified by its arguments.
        /// </summary>
        /// <param name="method">The <see cref="IMethod"/> for which to retrieve the argument Types.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>
        /// The Types of this <paramref name="method"/>'s <see cref="IGenericArgument"/>s, if they
        /// are not all implicitly specified by its arguments, otherwise an empty ReadOnlyCollection.
        /// </returns>
        public static ReadOnlyCollection<IGenericArgument> GetRequiredExplicitGenericArguments(
            this IMethod method,
            TranslationSettings settings)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<IGenericArgument>.EmptyReadOnlyCollection;
            }

            var methodGenericDefinition = method.GetGenericMethodDefinition();
            var genericArguments = methodGenericDefinition.GetGenericArguments();
            var requiredGenericParameterTypes = genericArguments.Project(arg => arg.Type).ToList();

            if (settings.UseImplicitGenericParameters)
            {
                RemoveSuppliedGenericTypeArguments(
                    methodGenericDefinition.GetParameters().Project(p => p.Type),
                    requiredGenericParameterTypes);
            }

            return requiredGenericParameterTypes.Count != 0
                ? genericArguments.ToReadOnlyCollection()
                : Enumerable<IGenericArgument>.EmptyReadOnlyCollection;
        }

        private static void RemoveSuppliedGenericTypeArguments(
            IEnumerable<Type> parameterTypes,
            ICollection<Type> genericParameterTypes)
        {
            foreach (var type in parameterTypes.Project(t => t.IsByRef ? t.GetElementType() : t))
            {
                if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                {
                    genericParameterTypes.Remove(type);
                }

                if (type.IsGenericType())
                {
                    RemoveSuppliedGenericTypeArguments(type.GetGenericTypeArguments(), genericParameterTypes);
                }
            }
        }
    }
}
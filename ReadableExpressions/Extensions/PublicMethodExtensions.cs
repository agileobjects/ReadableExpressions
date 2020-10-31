namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;
    using Translations.Reflection;

    /// <summary>
    /// Provides extension methods to use with <see cref="IMethod"/> implementations.
    /// </summary>
    public static class PublicMethodExtensions
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is an override.
        /// </summary>
        /// <param name="method">The <see cref="IMethod"/> for which to make the determination.</param>
        public static bool IsOverride(this IMethod method)
        {
            if (method.DeclaringType == null)
            {
                return false;
            }

            var parameterTypes = default(IList<Type>);

            foreach (var candidateMethod in GetOverridableMethods(method.DeclaringType.GetBaseType()))
            {
                if (candidateMethod.Name != method.Name)
                {
                    continue;
                }

                var parameters = candidateMethod.GetParameters();

                if (parameters.Length == 0)
                {
                    return true;
                }

                parameterTypes ??= method.GetParameters().Select(p => p.Type).ToList();

                if (parameterTypes.SequenceEqual(parameters.Project(p => p.ParameterType)))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<MethodInfo> GetOverridableMethods(Type type)
        {
            if (type == null)
            {
                yield break;
            }

            var candidateMethods = type
                .GetPublicInstanceMethods()
                .Concat(type.GetNonPublicInstanceMethods())
                .Concat(type.GetPublicInstanceProperties().SelectMany(p => p.GetAccessors()))
                .Concat(type.GetNonPublicInstanceProperties().SelectMany(p => p.GetAccessors()))
                .Concat(GetOverridableMethods(type.GetBaseType()))
                .Filter(m => m.IsAbstract || m.IsVirtual);

            foreach (var candidateMethod in candidateMethods)
            {
                yield return candidateMethod;
            }
        }

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

            var requiredGenericParameterTypes = methodGenericDefinition
                .GetGenericArguments()
                .Project(arg => arg.Type)
                .ToList();

            if (settings.UseImplicitGenericParameters)
            {
                RemoveSuppliedGenericTypeArguments(
                    methodGenericDefinition.GetParameters().Project(p => p.Type),
                    requiredGenericParameterTypes);
            }

            return requiredGenericParameterTypes.Count != 0
                ? method.GetGenericArguments().ToReadOnlyCollection()
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
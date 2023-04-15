namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
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
            if (method.IsStatic || method.DeclaringType == null)
            {
                return false;
            }

            var parameterTypes = default(IList<IType>);

            foreach (var candidateMethod in GetOverridableMethods(method.Name, method.DeclaringType))
            {
                var parameters = candidateMethod.GetParameters();

                if (parameters.None())
                {
                    return true;
                }

                parameterTypes ??= method.GetParameters().ProjectToArray(p => p.Type);

                if (parameterTypes.SequenceEqual(parameters.Project(p => p.Type)))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<IMethod> GetOverridableMethods(string name, IType type)
        {
            var baseType = type.BaseType;

            if (baseType == null)
            {
                yield break;
            }
           
            var candidateMethods = baseType
                .GetMembers(m => m.Instance().Methods().Named(name))
                .Cast<IMethod>()
                .Concat(baseType
                    .GetMembers(m => m.Instance().Properties().Named(name))
                    .Cast<IProperty>()
                    .SelectMany(p => p.GetAccessors()))
                .Concat(GetOverridableMethods(name, baseType))
                .Filter(m => m.IsAbstract || m.IsVirtual);

            foreach (var candidateMethod in candidateMethods)
            {
                yield return candidateMethod;
            }
        }

        /// <summary>
        /// Gets the Types of this <paramref name="method"/>'s <see cref="IGenericParameter"/>s, if
        /// they are not all implicitly specified by its arguments.
        /// </summary>
        /// <param name="method">The <see cref="IMethod"/> for which to retrieve the argument Types.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>
        /// The Types of this <paramref name="method"/>'s <see cref="IGenericParameter"/>s, if they
        /// are not all implicitly specified by its arguments, otherwise an empty ReadOnlyCollection.
        /// </returns>
        public static ReadOnlyCollection<IGenericParameter> GetRequiredExplicitGenericArguments(
            this IMethod method,
            TranslationSettings settings)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<IGenericParameter>.EmptyReadOnlyCollection;
            }

            var methodGenericDefinition = method.GetGenericMethodDefinition();

            var requiredGenericParameterTypes = methodGenericDefinition
                .GetGenericArguments()
                .Project<IGenericParameter, IType>(arg => arg)
                .ToList();

            if (settings.UseImplicitGenericParameters)
            {
                RemoveSuppliedGenericTypeArguments(
                    methodGenericDefinition.GetParameters().Project(p => p.Type),
                    requiredGenericParameterTypes);
            }

            return requiredGenericParameterTypes.Count != 0
                ? method.GetGenericArguments().ToReadOnlyCollection()
                : Enumerable<IGenericParameter>.EmptyReadOnlyCollection;
        }

        private static void RemoveSuppliedGenericTypeArguments(
            IEnumerable<IType> parameterTypes,
            ICollection<IType> genericParameterTypes)
        {
            foreach (var type in parameterTypes.Project(t => t.IsByRef ? t.ElementType : t))
            {
                if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                {
                    genericParameterTypes.Remove(type);
                }

                if (type.IsGeneric)
                {
                    RemoveSuppliedGenericTypeArguments(type.GenericTypeArguments, genericParameterTypes);
                }
            }
        }
    }
}
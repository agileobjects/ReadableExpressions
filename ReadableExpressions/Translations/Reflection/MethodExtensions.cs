namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using NetStandardPolyfills;

    internal static class MethodExtensions
    {
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

            return requiredGenericParameterTypes.Any()
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
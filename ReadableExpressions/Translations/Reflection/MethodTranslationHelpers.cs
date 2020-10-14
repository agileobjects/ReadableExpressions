namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
#if NETSTANDARD
    using NetStandardPolyfills;
#endif

    internal static class MethodTranslationHelpers
    {
        public static string GetAccessibility(PropertyInfo property)
        {
            var accessors = property.GetAccessors(nonPublic: true);

            if (accessors[0].IsPublic || accessors[1].IsPublic)
            {
                return "public ";
            }

            if (accessors[0].IsAssembly || accessors[1].IsAssembly)
            {
                return "internal ";
            }

            if (accessors[0].IsFamily || accessors[1].IsFamily)
            {
                return "protected ";
            }

            if (accessors[0].IsFamilyOrAssembly || accessors[1].IsFamilyOrAssembly)
            {
                return "protected internal ";
            }

            if (accessors[0].IsPrivate || accessors[1].IsPrivate)
            {
                return "private ";
            }

            return string.Empty;
        }

        public static string GetAccessibility(MethodInfo method, TranslationSettings settings)
            => GetAccessibility(new BclMethodWrapper(method, settings));

        public static string GetAccessibility(IMethod method)
        {
            if (method.IsPublic)
            {
                return "public ";
            }

            if (method.IsInternal)
            {
                return "internal ";
            }

            if (method.IsProtected)
            {
                return "protected ";
            }

            if (method.IsProtectedInternal)
            {
                return "protected internal ";
            }

            if (method.IsPrivate)
            {
                return "private ";
            }

            return string.Empty;
        }

        public static string GetModifiers(MethodInfo method, TranslationSettings settings)
            => GetModifiers(new BclMethodWrapper(method, settings));

        public static string GetModifiers(IMethod method)
        {
            if (method.IsAbstract)
            {
                return "abstract ";
            }

            if (method.IsStatic)
            {
                return "static ";
            }

            if (method.IsVirtual)
            {
                return "virtual ";
            }

            return string.Empty;
        }
    }
}

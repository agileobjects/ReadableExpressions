namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;

    internal static class MethodTranslationHelpers
    {
        public static string GetAccessibility(MethodBase method)
        {
            if (method.IsPublic)
            {
                return "public ";
            }

            if (method.IsAssembly)
            {
                return "internal ";
            }

            if (method.IsFamily)
            {
                return "protected ";
            }

            if (method.IsFamilyOrAssembly)
            {
                return "protected internal ";
            }

            if (method.IsPrivate)
            {
                return "private ";
            }

            return string.Empty;
        }

        public static string GetModifiers(MethodBase method)
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

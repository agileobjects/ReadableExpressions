namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
#if NETSTANDARD1_0
    using NetStandardPolyfills;
#endif

    internal static class MethodTranslationHelpers
    {
        public static string GetAccessibilityForTranslation(
            MethodInfo method,
            TranslationSettings settings)
        {
            return new ClrMethodWrapper(method, settings).GetAccessibilityForTranslation();
        }

        public static string GetAccessibilityForTranslation(this IMember member)
        {
            var accessibility = member.GetAccessibility();

            if (accessibility != string.Empty)
            {
                accessibility += ' ';
            }

            return accessibility;
        }

        public static string GetModifiersForTranslation(this IComplexMember member)
        {
            var modifiers = member.GetModifiers();

            if (modifiers != string.Empty)
            {
                modifiers += ' ';
            }

            return modifiers;
        }
    }
}

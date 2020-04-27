namespace AgileObjects.ReadableExpressions.Translations
{
    using Formatting;
    using Interfaces;

    internal static class WrappedTranslationExtensions
    {
        public static TranslationWrapper WithPrefix(
            this ITranslation translation,
            string prefix,
            TokenType tokenType)
        {
            return new TranslationWrapper(translation).WithPrefix(prefix, tokenType);
        }
    }
}
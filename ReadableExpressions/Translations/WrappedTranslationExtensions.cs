namespace AgileObjects.ReadableExpressions.Translations
{
    using Interfaces;

    internal static class WrappedTranslationExtensions
    {
        public static TranslationWrapper WithPrefix(
            this ITranslation translation,
            string prefix,
            TokenType tokenType = TokenType.Default)
        {
            return new TranslationWrapper(translation).WithPrefix(prefix, tokenType);
        }

        public static TranslationWrapper WithSuffix(
            this ITranslation translation,
            string suffix,
            TokenType tokenType = TokenType.Default)
        {
            return new TranslationWrapper(translation).WithSuffix(suffix, tokenType);
        }
    }
}
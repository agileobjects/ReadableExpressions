namespace AgileObjects.ReadableExpressions.Translations
{
    using Extensions;
    using Formatting;

    internal static class TranslationContextExtensions
    {
        public static int GetKeywordFormattingSize(this TranslationSettings settings)
            => settings.GetFormattingSize(TokenType.Keyword);

        public static int GetControlStatementFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.ControlStatement);

        public static int GetTypeNameFormattingSize(this ITranslationContext context)
            => context.Settings.GetTypeNameFormattingSize();

        public static int GetTypeNameFormattingSize(this TranslationSettings settings)
            => settings.GetFormattingSize(TokenType.TypeName);
        public static int GetFormattingSize(this TranslationSettings settings, TokenType tokenType)
            => settings.Formatter.GetFormattingSize(tokenType);
    }
}
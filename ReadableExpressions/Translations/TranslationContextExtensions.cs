namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;
    using Interfaces;

    internal static class TranslationContextExtensions
    {
        public static TypeNameTranslation GetTranslationFor(
            this ITranslationContext context, 
            Type type)
        {
            return new TypeNameTranslation(type, context);
        }

        public static CodeBlockTranslation GetCodeBlockTranslationFor(
            this ITranslationContext context, 
            Expression expression)
        {
            return new CodeBlockTranslation(context.GetTranslationFor(expression), context);
        }

        public static int GetKeywordFormattingSize(this ITranslationContext context)
            => context.Settings.GetKeywordFormattingSize();

        public static int GetKeywordFormattingSize(this ITranslationSettings settings)
            => settings.GetFormattingSize(TokenType.Keyword);

        public static int GetControlStatementFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.ControlStatement);

        public static int GetTypeNameFormattingSize(this ITranslationContext context)
            => context.Settings.GetTypeNameFormattingSize();

        public static int GetTypeNameFormattingSize(this ITranslationSettings settings)
            => settings.GetFormattingSize(TokenType.TypeName);

        public static int GetFormattingSize(this ITranslationContext context, TokenType tokenType)
            => context.Settings.GetFormattingSize(tokenType);

        public static int GetFormattingSize(this ITranslationSettings settings, TokenType tokenType)
            => settings.Formatter.GetFormattingSize(tokenType);
    }
}
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
            => context.GetFormattingSize(TokenType.Keyword);

        public static int GetControlStatementFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.ControlStatement);

        public static int GetTypeNameFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.TypeName);

        public static int GetVariableFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.Variable);

        public static int GetNumericFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.Numeric);

        public static int GetFormattingSize(this ITranslationContext context, TokenType tokenType)
            => context.Settings.Formatter.GetFormattingSize(tokenType);
    }
}
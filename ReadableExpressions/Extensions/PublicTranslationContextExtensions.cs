namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Translations;
    using Translations.Formatting;
    using Translations.Interfaces;

    /// <summary>
    /// Provides extension methods to use with a <see cref="ITranslationContext"/>.
    /// </summary>
    public static class PublicTranslationContextExtensions
    {
        /// <summary>
        /// Gets a <see cref="CodeBlockTranslation"/> for the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> with which to create the <see cref="ITranslation"/>.
        /// </param>
        /// <param name="expression">The Expression for which to get a <see cref="CodeBlockTranslation"/>.</param>
        /// <returns>A <see cref="CodeBlockTranslation"/> for the given <paramref name="expression"/>.</returns>
        public static CodeBlockTranslation GetCodeBlockTranslationFor(
            this ITranslationContext context,
            Expression expression)
        {
            return new CodeBlockTranslation(context.GetTranslationFor(expression), context);
        }

        /// <summary>
        /// Gets the <see cref="ITranslation"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> with which to create the <see cref="ITranslation"/>.
        /// </param>
        /// <param name="type">The Type for which to get an <see cref="ITranslation"/>.</param>
        /// <returns>The <see cref="ITranslation"/> for the given <paramref name="type"/>.</returns>
        public static TypeNameTranslation GetTranslationFor(
            this ITranslationContext context,
            Type type)
        {
            return new TypeNameTranslation(type, context.Settings);
        }

        /// <summary>
        /// Gets the number of characters used by keyword formatting in this <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> containing the <see cref="TranslationSettings"/>
        /// which specify the formatting size.
        /// </param>
        /// <returns>The number of characters used by keyword formatting.</returns>
        public static int GetKeywordFormattingSize(this ITranslationContext context)
            => context.Settings.GetKeywordFormattingSize();

        /// <summary>
        /// Gets the number of characters used by a the formatting of a token of the given
        /// <paramref name="tokenType"/> in this <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> containing the <see cref="TranslationSettings"/>
        /// which specify the formatting size.
        /// </param>
        /// <param name="tokenType">The <see cref="TokenType"/> for which to retrieve the formatting size.</param>
        /// <returns>The number of characters used to format a token of the given <paramref name="tokenType"/>.</returns>
        public static int GetFormattingSize(this ITranslationContext context, TokenType tokenType)
            => context.Settings.GetFormattingSize(tokenType);
    }
}
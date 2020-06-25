namespace AgileObjects.ReadableExpressions
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System;
    using SourceCode;

    /// <summary>
    /// A factory class providing source code Expressions.
    /// </summary>
    public static class ReadableExpression
    {
        /// <summary>
        /// Create a <see cref="SourceCodeExpression"/> representing a complete piece of source code
        /// with the given <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The content of the piece of source code to create.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="SourceCodeExpression"/> representing a complete piece of source code.</returns>
        public static SourceCodeExpression SourceCode(
            Expression content,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration = null)
        {
            if (content == null)
            {
                return null;
            }

            return content.ToSourceCodeExpression(configuration, out _);
        }

        internal static SourceCodeExpression ToSourceCodeExpression(
            this Expression content,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            return content.ToSourceCodeExpression(
                (cnt, stgs) => new SourceCodeExpression(cnt, stgs),
                configuration,
                out settings);
        }

        /// <summary>
        /// Create a <see cref="ClassExpression"/> representing a source code class with the given
        /// <paramref name="singleMethod"/>.
        /// </summary>
        /// <param name="singleMethod">
        /// An Expression defining the <see cref="ClassExpression"/>'s single method.
        /// </param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="ClassExpression"/> representing a source code class.</returns>
        public static ClassExpression Class(
            Expression singleMethod,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration = null)
        {
            if (singleMethod == null)
            {
                return null;
            }

            return singleMethod.ToClassExpression(configuration, out _);
        }

        internal static ClassExpression ToClassExpression(
            this Expression content,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            return content.ToSourceCodeExpression(
                (cnt, stgs) => new ClassExpression(cnt, stgs),
                configuration,
                out settings);
        }

        /// <summary>
        /// Create a <see cref="MethodExpression"/> representing a source code method with the given
        /// <paramref name="method"/> definition.
        /// </summary>
        /// <param name="method">An Expression defining the <see cref="MethodExpression"/>.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="MethodExpression"/> representing a source code method.</returns>
        public static MethodExpression Method(
            Expression method,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration = null)
        {
            if (method == null)
            {
                return null;
            }

            return method.ToMethodExpression(configuration, out _);
        }

        internal static MethodExpression ToMethodExpression(
            this Expression content,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            return content.ToSourceCodeExpression(MethodExpression.For, configuration, out settings);
        }

        /// <summary>
        /// Create a <see cref="CommentExpression"/> representing a code comment with the 
        /// given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text of the comment to create, without slashes or /* */.</param>
        /// <returns>A <see cref="CommentExpression"/> representing a code comment.</returns>
        public static CommentExpression Comment(string text)
            => new CommentExpression(text);

        private static TExpression ToSourceCodeExpression<TExpression>(
            this Expression content,
            Func<Expression, TranslationSettings, TExpression> expressionFactory,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            settings = configuration.GetTranslationSettings();

            return expressionFactory.Invoke(content, settings);
        }

        private static TranslationSettings GetTranslationSettings(
            this Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration)
        {
            if (configuration == null)
            {
                return TranslationSettings.DefaultSourceCode;
            }

            var settings = TranslationSettings.ForSourceCode();

            configuration.Invoke(settings);

            return settings;
        }
    }
}

namespace AgileObjects.ReadableExpressions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using SourceCode;
    using SourceCode.Api;

    /// <summary>
    /// A factory class providing source code Expressions.
    /// </summary>
    public static class ReadableExpression
    {
        /// <summary>
        /// Create a <see cref="SourceCodeExpression"/> representing a complete piece of source code
        /// using the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">
        /// The configuration with which to generate the <see cref="SourceCodeExpression"/>.
        /// </param>
        /// <returns>A <see cref="SourceCodeExpression"/> representing a complete piece of source code.</returns>
        public static SourceCodeExpression SourceCode(
            Func<ISourceCodeExpressionSettings, ISourceCodeExpressionSettings> configuration)
        {
            var builder = new SourceCodeExpressionBuilder();
            configuration.Invoke(builder);

            return builder.Build();
        }

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
            return content?.ToSourceCodeExpression(configuration);
        }

        internal static SourceCodeExpression ToSourceCodeExpression(
            this Expression content,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration)
        {
            var settings = GetTranslationSettings(configuration, (cfg, s) => cfg.Invoke(s));

            return new SourceCodeExpression(content, settings);
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
            Func<IClassTranslationSettings, IClassTranslationSettings> configuration = null)
        {
            return singleMethod?.ToClassExpression(configuration, out _);
        }

        internal static ClassExpression ToClassExpression(
            this Expression content,
            Func<IClassTranslationSettings, IClassTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            settings = GetTranslationSettings(configuration, (cfg, s) => cfg.Invoke(s));

            return new ClassExpression(content, settings);
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
            Func<IMethodTranslationSettings, IMethodTranslationSettings> configuration = null)
        {
            return method?.ToMethodExpression(configuration, out _);
        }

        internal static MethodExpression ToMethodExpression(
            this Expression content,
            Func<IMethodTranslationSettings, IMethodTranslationSettings> configuration,
            out TranslationSettings settings)
        {
            settings = GetTranslationSettings(configuration, (cfg, s) => cfg.Invoke(s));

            return MethodExpression.For(content, settings);
        }

        /// <summary>
        /// Create a <see cref="CommentExpression"/> representing a code comment with the 
        /// given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text of the comment to create, without slashes or /* */.</param>
        /// <returns>A <see cref="CommentExpression"/> representing a code comment.</returns>
        public static CommentExpression Comment(string text)
            => new CommentExpression(text);

        private static TranslationSettings GetTranslationSettings<TConfiguration>(
            this TConfiguration configuration,
            Action<TConfiguration, TranslationSettings> configurator)
        {
            if (configuration == null)
            {
                return TranslationSettings.DefaultSourceCode;
            }

            var settings = TranslationSettings.ForSourceCode();

            configurator.Invoke(configuration, settings);
            return settings;
        }
    }
}

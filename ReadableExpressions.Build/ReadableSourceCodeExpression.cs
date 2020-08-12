namespace AgileObjects.ReadableExpressions.Build
{
    using System;
    using System.Linq.Expressions;
    using SourceCode;
    using SourceCode.Api;

    /// <summary>
    /// A factory class providing source code Expressions.
    /// </summary>
    public static class ReadableSourceCodeExpression
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
        /// Create a <see cref="CommentExpression"/> representing a code comment with the 
        /// given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text of the comment to create, without slashes or /* */.</param>
        /// <returns>A <see cref="CommentExpression"/> representing a code comment.</returns>
        public static CommentExpression Comment(string text) => ReadableExpression.Comment(text);

        private static SourceCodeTranslationSettings GetTranslationSettings<TConfiguration>(
            this TConfiguration configuration,
            Action<TConfiguration, SourceCodeTranslationSettings> configurator)
        {
            if (configuration == null)
            {
                return SourceCodeTranslationSettings.Default;
            }

            var settings = SourceCodeTranslationSettings.Create();

            configurator.Invoke(configuration, settings);
            return settings;
        }
    }
}

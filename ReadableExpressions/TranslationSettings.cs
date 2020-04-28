namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations.Formatting;

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    public class TranslationSettings
    {
        internal static readonly TranslationSettings Default = new TranslationSettings();

        internal TranslationSettings()
        {
            UseImplicitGenericParameters = true;
            Formatter = NullTranslationFormatter.Insance;
        }

        /// <summary>
        /// Fully qualify Type names with their namespace.
        /// </summary>
        public TranslationSettings UseFullyQualifiedTypeNames
        {
            get
            {
                FullyQualifyTypeNames = true;
                return this;
            }
        }

        internal bool FullyQualifyTypeNames { get; private set; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        public TranslationSettings UseExplicitGenericParameters
        {
            get
            {
                UseImplicitGenericParameters = false;
                return this;
            }
        }

        internal bool UseImplicitGenericParameters { get; private set; }

        /// <summary>
        /// Annotate a Quoted Lambda Expression with a comment indicating that it has 
        /// been Quoted.
        /// </summary>
        public TranslationSettings ShowQuotedLambdaComments
        {
            get
            {
                CommentQuotedLambdas = true;
                return this;
            }
        }

        internal bool DoNotCommentQuotedLambdas => !CommentQuotedLambdas;

        internal bool CommentQuotedLambdas { get; set; }

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>This <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings NameAnonymousTypesUsing(Func<Type, string> nameFactory)
        {
            AnonymousTypeNameFactory = nameFactory;
            return this;
        }

        internal Func<Type, string> AnonymousTypeNameFactory { get; private set; }

        /// <summary>
        /// Translate ConstantExpressions using the given <paramref name="valueFactory"/> instead of
        /// the default method.
        /// </summary>
        /// <param name="valueFactory">
        /// The factory method to execute to retrieve the ConstantExpression's translated value.
        /// </param>
        /// <returns>This <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory)
        {
            ConstantExpressionValueFactory = valueFactory;
            return this;
        }

        internal Func<Type, object, string> ConstantExpressionValueFactory { get; private set; }

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>This <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        internal ITranslationFormatter Formatter { get; private set; }
    }
}

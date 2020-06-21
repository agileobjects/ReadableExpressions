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

        private bool _commentQuotedLambdas;
        private int? _indentLength;
        private ExpressionAnalysis _emptyAnalysis;

        internal TranslationSettings()
        {
            UseImplicitTypeNames = true;
            UseImplicitGenericParameters = true;
            HideImplicitlyTypedArrayTypes = true;
            Indent = "    ";
            Formatter = NullTranslationFormatter.Instance;
        }

        internal ExpressionAnalysis EmptyAnalysis =>
            _emptyAnalysis ??= new ExpressionAnalysis(this).Finalise();

        /// <summary>
        /// Fully qualify type names with their namespaces.
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
        /// Use full type names instead of 'var' for local and inline-declared output parameter variables.
        /// </summary>
        public TranslationSettings UseExplicitTypeNames
        {
            get
            {
                UseImplicitTypeNames = false;
                return this;
            }
        }

        internal bool UseImplicitTypeNames { get; private set; }

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
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        public TranslationSettings DeclareOutputParametersInline
        {
            get
            {
                DeclareOutParamsInline = true;
                return this;
            }
        }

        internal bool DeclareOutParamsInline { get; private set; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        public TranslationSettings ShowImplicitArrayTypes
        {
            get
            {
                HideImplicitlyTypedArrayTypes = false;
                return this;
            }
        }

        internal bool HideImplicitlyTypedArrayTypes { get; private set; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        public TranslationSettings ShowLambdaParameterTypes
        {
            get
            {
                ShowLambdaParamTypes = true;
                return this;
            }
        }

        internal bool ShowLambdaParamTypes { get; private set; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        public TranslationSettings ShowQuotedLambdaComments
        {
            get
            {
                _commentQuotedLambdas = true;
                return this;
            }
        }

        internal bool DoNotCommentQuotedLambdas => !_commentQuotedLambdas;

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
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
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory)
        {
            ConstantExpressionValueFactory = valueFactory;
            return this;
        }

        internal Func<Type, object, string> ConstantExpressionValueFactory { get; private set; }

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings IndentUsing(string indent)
        {
            Indent = indent;
            return this;
        }

        internal string Indent { get; private set; }

        internal int IndentLength => _indentLength ??= Indent.Length;

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TranslationSettings FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        internal ITranslationFormatter Formatter { get; private set; }
    }
}

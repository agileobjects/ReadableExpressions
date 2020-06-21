namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations.Formatting;

    /// <summary>
    /// Provides common configuration options to control aspects of source-code string generation.
    /// </summary>
    public abstract class TranslationSettingsBase<TSettings> : ITranslationSettings
    {
        private bool _commentQuotedLambdas;
        private int? _indentLength;
        private ExpressionAnalysis _emptyAnalysis;

        internal TranslationSettingsBase()
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
        public TSettings UseFullyQualifiedTypeNames
        {
            get
            {
                FullyQualifyTypeNames = true;
                return Settings;
            }
        }

        internal bool FullyQualifyTypeNames { get; private set; }

        /// <summary>
        /// Use full type names instead of 'var' for local and inline-declared output parameter variables.
        /// </summary>
        public TSettings UseExplicitTypeNames
        {
            get
            {
                UseImplicitTypeNames = false;
                return Settings;
            }
        }

        internal bool UseImplicitTypeNames { get; private set; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        public TSettings UseExplicitGenericParameters
        {
            get
            {
                UseImplicitGenericParameters = false;
                return Settings;
            }
        }

        internal bool UseImplicitGenericParameters { get; private set; }

        /// <summary>
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        public TSettings DeclareOutputParametersInline
        {
            get
            {
                DeclareOutParamsInline = true;
                return Settings;
            }
        }

        bool ITranslationSettings.DeclareOutParamsInline => DeclareOutParamsInline;

        internal bool DeclareOutParamsInline { get; private set; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        public TSettings ShowImplicitArrayTypes
        {
            get
            {
                HideImplicitlyTypedArrayTypes = false;
                return Settings;
            }
        }

        internal bool HideImplicitlyTypedArrayTypes { get; private set; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        public TSettings ShowLambdaParameterTypes
        {
            get
            {
                ShowLambdaParamTypes = true;
                return Settings;
            }
        }

        internal bool ShowLambdaParamTypes { get; private set; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        public TSettings ShowQuotedLambdaComments
        {
            get
            {
                _commentQuotedLambdas = true;
                return Settings;
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
        public TSettings NameAnonymousTypesUsing(Func<Type, string> nameFactory)
        {
            AnonymousTypeNameFactory = nameFactory;
            return Settings;
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
        public TSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory)
        {
            ConstantExpressionValueFactory = valueFactory;
            return Settings;
        }

        internal Func<Type, object, string> ConstantExpressionValueFactory { get; private set; }

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public TSettings IndentUsing(string indent)
        {
            Indent = indent;
            return Settings;
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
        public TSettings FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return Settings;
        }

        internal ITranslationFormatter Formatter { get; private set; }

        internal abstract TSettings Settings { get; }
    }
}
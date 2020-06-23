namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations.Formatting;

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    public interface ITranslationSettings
    {
        /// <summary>
        /// Fully qualify type names with their namespaces.
        /// </summary>
        ITranslationSettings UseFullyQualifiedTypeNames { get; }

        /// <summary>
        /// Use full type names instead of 'var' for local and inline-declared output parameter
        /// variables.
        /// </summary>
        ITranslationSettings UseExplicitTypeNames { get; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        ITranslationSettings UseExplicitGenericParameters { get; }

        /// <summary>
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        ITranslationSettings DeclareOutputParametersInline { get; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        ITranslationSettings ShowImplicitArrayTypes { get; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        ITranslationSettings ShowLambdaParameterTypes { get; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        ITranslationSettings ShowQuotedLambdaComments { get; }

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings NameAnonymousTypesUsing(Func<Type, string> nameFactory);

        /// <summary>
        /// Translate ConstantExpressions using the given <paramref name="valueFactory"/> instead of
        /// the default method.
        /// </summary>
        /// <param name="valueFactory">
        /// The factory method to execute to retrieve the ConstantExpression's translated value.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory);

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings IndentUsing(string indent);

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings FormatUsing(ITranslationFormatter formatter);
    }

    /// <summary>
    /// Provides configuration options to control aspects of source-code generation.
    /// </summary>
    public interface ISourceCodeTranslationSettings : ITranslationSettings
    {

    }

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    internal class TranslationSettings : ISourceCodeTranslationSettings
    {
        public static readonly TranslationSettings Default = new TranslationSettings();

        public static readonly TranslationSettings DefaultSourceCode = new TranslationSettings
        {
            CollectRequiredNamespaces = true
        };

        private bool _commentQuotedLambdas;
        private int? _indentLength;

        public TranslationSettings()
        {
            UseImplicitTypeNames = true;
            UseImplicitGenericParameters = true;
            HideImplicitlyTypedArrayTypes = true;
            Indent = "    ";
            Formatter = NullTranslationFormatter.Instance;
        }

        public bool CollectRequiredNamespaces { get; set; }

        /// <summary>
        /// Fully qualify type names with their namespaces.
        /// </summary>
        public ITranslationSettings UseFullyQualifiedTypeNames
        {
            get
            {
                FullyQualifyTypeNames = true;
                return this;
            }
        }

        public bool FullyQualifyTypeNames { get; private set; }

        /// <summary>
        /// Use full type names instead of 'var' for local and inline-declared output parameter variables.
        /// </summary>
        public ITranslationSettings UseExplicitTypeNames
        {
            get
            {
                UseImplicitTypeNames = false;
                return this;
            }
        }

        public bool UseImplicitTypeNames { get; private set; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        public ITranslationSettings UseExplicitGenericParameters
        {
            get
            {
                UseImplicitGenericParameters = false;
                return this;
            }
        }

        public bool UseImplicitGenericParameters { get; private set; }

        /// <summary>
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        public ITranslationSettings DeclareOutputParametersInline
        {
            get
            {
                DeclareOutParamsInline = true;
                return this;
            }
        }

        public bool DeclareOutParamsInline { get; private set; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        public ITranslationSettings ShowImplicitArrayTypes
        {
            get
            {
                HideImplicitlyTypedArrayTypes = false;
                return this;
            }
        }

        public bool HideImplicitlyTypedArrayTypes { get; private set; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        public ITranslationSettings ShowLambdaParameterTypes
        {
            get
            {
                ShowLambdaParamTypes = true;
                return this;
            }
        }

        public bool ShowLambdaParamTypes { get; private set; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        public ITranslationSettings ShowQuotedLambdaComments
        {
            get
            {
                _commentQuotedLambdas = true;
                return this;
            }
        }

        public bool DoNotCommentQuotedLambdas => !_commentQuotedLambdas;

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public ITranslationSettings NameAnonymousTypesUsing(Func<Type, string> nameFactory)
        {
            AnonymousTypeNameFactory = nameFactory;
            return this;
        }

        public Func<Type, string> AnonymousTypeNameFactory { get; private set; }

        /// <summary>
        /// Translate ConstantExpressions using the given <paramref name="valueFactory"/> instead of
        /// the default method.
        /// </summary>
        /// <param name="valueFactory">
        /// The factory method to execute to retrieve the ConstantExpression's translated value.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public ITranslationSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory)
        {
            ConstantExpressionValueFactory = valueFactory;
            return this;
        }

        public Func<Type, object, string> ConstantExpressionValueFactory { get; private set; }

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public ITranslationSettings IndentUsing(string indent)
        {
            Indent = indent;
            return this;
        }

        public string Indent { get; private set; }

        public int IndentLength => _indentLength ??= Indent.Length;

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        public ITranslationSettings FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        public ITranslationFormatter Formatter { get; private set; }
    }
}

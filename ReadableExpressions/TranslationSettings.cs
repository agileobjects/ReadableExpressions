namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations.Formatting;

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    public class TranslationSettings : ITranslationSettings
    {
        internal static readonly TranslationSettings Default = new TranslationSettings();

        private bool _commentQuotedLambdas;
        private int? _indentLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationSettings"/> class.
        /// </summary>
        protected TranslationSettings()
        {
            UseImplicitTypeNames = true;
            UseImplicitGenericParameters = true;
            HideImplicitlyTypedArrayTypes = true;
            Indent = "    ";
            Formatter = NullTranslationFormatter.Instance;
        }

        /// <summary>
        /// Fully qualify type names with their namespaces.
        /// </summary>
        ITranslationSettings ITranslationSettings.UseFullyQualifiedTypeNames
        {
            get
            {
                FullyQualifyTypeNames = true;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether type names should be fully-qualified with their namespace.
        /// </summary>
        public bool FullyQualifyTypeNames { get; private set; }

        /// <summary>
        /// Use full type names instead of 'var' for local and inline-declared output parameter variables.
        /// </summary>
        ITranslationSettings ITranslationSettings.UseExplicitTypeNames
        {
            get
            {
                UseImplicitTypeNames = false;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether 'var' should be used instead of full type names local
        /// and inline-declared output parameter variables.
        /// </summary>
        public bool UseImplicitTypeNames { get; private set; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        ITranslationSettings ITranslationSettings.UseExplicitGenericParameters
        {
            get
            {
                UseImplicitGenericParameters = false;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether implicitly-typed generic parameter arguments should be
        /// excluded from method calls when possible.
        /// </summary>
        public bool UseImplicitGenericParameters { get; private set; }

        /// <summary>
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        ITranslationSettings ITranslationSettings.DeclareOutputParametersInline
        {
            get
            {
                DeclareOutParamsInline = true;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether output parameter varables should be declared inline with
        /// the method call where they are first used.
        /// </summary>
        public bool DeclareOutParamsInline { get; private set; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        ITranslationSettings ITranslationSettings.ShowImplicitArrayTypes
        {
            get
            {
                HideImplicitlyTypedArrayTypes = false;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the element type names of implicitly-typed arrays should
        /// be hidden.
        /// </summary>
        public bool HideImplicitlyTypedArrayTypes { get; private set; }

        /// <summary>
        /// Show the names of Lambda Expression parameter types.
        /// </summary>
        ITranslationSettings ITranslationSettings.ShowLambdaParameterTypes
        {
            get
            {
                ShowLambdaParamTypes = true;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the names of Lambda Expression parameter types should
        /// be shown.
        /// </summary>
        public bool ShowLambdaParamTypes { get; private set; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        ITranslationSettings ITranslationSettings.ShowQuotedLambdaComments
        {
            get
            {
                _commentQuotedLambdas = true;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Quoted Lambda Expressions not should be annotated with
        /// a comment indicating they have been Quoted.
        /// </summary>
        public bool DoNotCommentQuotedLambdas => !_commentQuotedLambdas;

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings ITranslationSettings.NameAnonymousTypesUsing(Func<Type, string> nameFactory)
        {
            AnonymousTypeNameFactory = nameFactory;
            return this;
        }

        /// <summary>
        /// Gets a factory to use to name anonymous types instead of the default method.
        /// </summary>
        public Func<Type, string> AnonymousTypeNameFactory { get; private set; }

        /// <summary>
        /// Translate ConstantExpression values using the given <paramref name="valueFactory"/>
        /// instead of the default method.
        /// </summary>
        /// <param name="valueFactory">
        /// The factory method to execute to retrieve the ConstantExpression's translated value.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings ITranslationSettings.TranslateConstantsUsing(Func<Type, object, string> valueFactory)
        {
            ConstantExpressionValueFactory = valueFactory;
            return this;
        }

        /// <summary>
        /// Gets a factory to use to translate ConstantExpression values instead of the default method.
        /// </summary>
        public Func<Type, object, string> ConstantExpressionValueFactory { get; private set; }

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings ITranslationSettings.IndentUsing(string indent)
        {
            Indent = indent;
            return this;
        }

        /// <summary>
        /// Gets the whitespace string with which to indent multi-line Expression translations.
        /// </summary>
        public string Indent { get; private set; }

        /// <summary>
        /// Gets the number of characters of whitespace with which to indent multi-line Expression
        /// translations.
        /// </summary>
        public int IndentLength => _indentLength ??= Indent.Length;

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>These <see cref="TranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings ITranslationSettings.FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        /// <summary>
        /// Gats the <see cref="ITranslationFormatter"/> with which to format the translation.
        /// </summary>
        public ITranslationFormatter Formatter { get; private set; }
    }
}

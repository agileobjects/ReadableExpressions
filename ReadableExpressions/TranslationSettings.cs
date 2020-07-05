namespace AgileObjects.ReadableExpressions
{
    using System;
    using SourceCode;
    using SourceCode.Api;
    using Translations.Formatting;

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    internal class TranslationSettings :
        ISourceCodeTranslationSettings,
        IClassTranslationSettings,
        IMethodTranslationSettings
    {
        public static readonly TranslationSettings Default = new TranslationSettings();

        public static readonly TranslationSettings DefaultSourceCode = ForSourceCode();

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

        #region Factory Methods

        public static TranslationSettings ForSourceCode()
            => SetDefaultSourceCodeOptions(new TranslationSettings());

        public static TranslationSettings SetDefaultSourceCodeOptions(TranslationSettings settings)
        {
            settings.CollectRequiredNamespaces = true;
            settings.Namespace = "GeneratedExpressionCode";
            settings.ClassNameFactory = (sc, classCtx) => sc.GetClassName(classCtx);
            settings.MethodNameFactory = (sc, cls, methodCtx) => cls.GetMethodName(methodCtx);

            return settings;
        }

        #endregion 

        public bool CollectRequiredNamespaces { get; private set; }

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

        public bool HideImplicitlyTypedArrayTypes { get; private set; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        ITranslationSettings ITranslationSettings.ShowLambdaParameterTypes
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
        ITranslationSettings ITranslationSettings.ShowQuotedLambdaComments
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
        ITranslationSettings ITranslationSettings.NameAnonymousTypesUsing(Func<Type, string> nameFactory)
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
        ITranslationSettings ITranslationSettings.TranslateConstantsUsing(Func<Type, object, string> valueFactory)
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
        ITranslationSettings ITranslationSettings.IndentUsing(string indent)
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
        ITranslationSettings ITranslationSettings.FormatUsing(ITranslationFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        public ITranslationFormatter Formatter { get; private set; }

        ISourceCodeTranslationSettings ISourceCodeTranslationSettings.WithNamespaceOf<T>()
            => SetNamespace(typeof(T).Namespace);

        ISourceCodeTranslationSettings ISourceCodeTranslationSettings.WithNamespace(string @namespace)
            => SetNamespace(@namespace);

        protected ISourceCodeTranslationSettings SetNamespace(string @namespace)
        {
            Namespace = @namespace;
            return this;
        }

        public string Namespace { get; private set; }

        #region Class Naming

        IClassTranslationSettings IClassTranslationSettings<IClassTranslationSettings>.NameClassesUsing(
            Func<IClassNamingContext, string> nameFactory)
        {
            return SetClassNamingFactory((sc, @class) => nameFactory.Invoke(@class));
        }

        public ISourceCodeTranslationSettings NameClassesUsing(
            Func<SourceCodeExpression, IClassNamingContext, string> nameFactory)
        {
            return SetClassNamingFactory(nameFactory);
        }

        private TranslationSettings SetClassNamingFactory(
            Func<SourceCodeExpression, IClassNamingContext, string> nameFactory)
        {
            ClassNameFactory = nameFactory;
            return this;
        }

        public ISourceCodeTranslationSettings NameClassesUsing(Func<IClassNamingContext, string> nameFactory)
        {
            ClassNameFactory = (sc, exp) => nameFactory.Invoke(exp);
            return this;
        }

        public Func<SourceCodeExpression, IClassNamingContext, string> ClassNameFactory { get; private set; }

        #endregion

        #region Method Naming

        public ISourceCodeTranslationSettings NameMethodsUsing(
            Func<SourceCodeExpression, ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        ISourceCodeTranslationSettings IClassTranslationSettings<ISourceCodeTranslationSettings>.NameMethodsUsing(
            Func<ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        IClassTranslationSettings IClassTranslationSettings<IClassTranslationSettings>.NameMethodsUsing(
            Func<ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        IClassTranslationSettings IMethodTranslationSettings<IClassTranslationSettings>.NameMethodsUsing(
            Func<IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        IMethodTranslationSettings IMethodTranslationSettings<IMethodTranslationSettings>.NameMethodsUsing(
            Func<IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        public ISourceCodeTranslationSettings NameMethodsUsing(
            Func<IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(nameFactory);
        }

        private TranslationSettings SetMethodNamingFactory(
            Func<IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory((cCtx, mCtx) => nameFactory.Invoke(mCtx));
        }

        private TranslationSettings SetMethodNamingFactory(
            Func<ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(
                (sc, cls, ctx) => nameFactory.Invoke(cls, ctx));
        }

        private TranslationSettings SetMethodNamingFactory(
            Func<SourceCodeExpression, ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            MethodNameFactory = nameFactory;
            return this;
        }

        public Func<SourceCodeExpression, ClassExpression, IMethodNamingContext, string> MethodNameFactory
        {
            get;
            private set;
        }

        #endregion

        ISourceCodeTranslationSettings ISourceCodeTranslationSettings.CreateSingleClass
        {
            get
            {
                GenerateSingleClass = true;
                return this;
            }
        }

        internal bool GenerateSingleClass { get; private set; }
    }
}

namespace AgileObjects.ReadableExpressions.Build
{
    using System;
    using SourceCode;
    using SourceCode.Api;

    internal class SourceCodeTranslationSettings :
        TranslationSettings,
        ISourceCodeTranslationSettings,
        IClassTranslationSettings,
        IMethodTranslationSettings
    {
        public new static readonly SourceCodeTranslationSettings Default = Create();

        #region Factory Methods

        public static SourceCodeTranslationSettings Create()
            => SetDefaultSourceCodeOptions(new SourceCodeTranslationSettings());

        public static SourceCodeTranslationSettings SetDefaultSourceCodeOptions(SourceCodeTranslationSettings settings)
        {
            settings.CollectRequiredNamespaces = true;
            settings.CollectInlineBlocks = true;
            settings.Namespace = "GeneratedExpressionCode";
            settings.ClassNameFactory = (sc, classCtx) => sc.GetClassName(classCtx);
            settings.MethodNameFactory = (sc, cls, methodCtx) => cls.GetMethodName(methodCtx);

            return settings;
        }

        #endregion

        public bool CollectRequiredNamespaces { get; private set; }

        public bool CollectInlineBlocks { get; private set; }

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

        private SourceCodeTranslationSettings SetClassNamingFactory(
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

        private SourceCodeTranslationSettings SetMethodNamingFactory(
            Func<IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory((cCtx, mCtx) => nameFactory.Invoke(mCtx));
        }

        private SourceCodeTranslationSettings SetMethodNamingFactory(
            Func<ClassExpression, IMethodNamingContext, string> nameFactory)
        {
            return SetMethodNamingFactory(
                (sc, cls, ctx) => nameFactory.Invoke(cls, ctx));
        }

        private SourceCodeTranslationSettings SetMethodNamingFactory(
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

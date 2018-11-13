namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal abstract class InitialisationTranslationBase<TInitializer> : ITranslation
    {
        private readonly ITranslation _newingTranslation;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            InitializerSetTranslationBase<TInitializer> initializerTranslations,
            ITranslationContext context)
            : this(
                initType,
                NewingTranslation.For(newing, context, omitParenthesesIfParameterless: initializerTranslations.Count != 0),
                initializerTranslations)
        {
        }

        protected InitialisationTranslationBase(
            ExpressionType initType,
            ITranslation newingTranslation,
            InitializerSetTranslationBase<TInitializer> initializerTranslations)
        {
            NodeType = initType;
            _newingTranslation = newingTranslation;
            InitializerTranslations = initializerTranslations;
            EstimatedSize = newingTranslation.EstimatedSize + initializerTranslations.EstimatedSize;

            initializerTranslations.IsLongTranslation = EstimatedSize > 40;
        }

        protected static bool InitHasNoInitializers(
            NewExpression newing,
            ICollection<TInitializer> initializers,
            ITranslationContext context,
            out ITranslation newingTranslation)
        {
            var hasInitializers = initializers.Count != 0;

            newingTranslation = NewingTranslation.For(
                newing,
                context,
                omitParenthesesIfParameterless: hasInitializers);

            return hasInitializers == false;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        protected InitializerSetTranslationBase<TInitializer> InitializerTranslations { get; }

        public void WriteTo(ITranslationContext context)
        {
            _newingTranslation.WriteTo(context);
            InitializerTranslations.WriteTo(context);
        }
    }
}
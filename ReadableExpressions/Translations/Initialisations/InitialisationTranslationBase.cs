namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

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
            TranslationSize = newingTranslation.TranslationSize + initializerTranslations.TranslationSize;
            FormattingSize = newingTranslation.FormattingSize + initializerTranslations.FormattingSize;

            initializerTranslations.IsLongTranslation = TranslationSize > 40;
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

        public Type Type => _newingTranslation.Type;

        public int TranslationSize { get; }
        
        public int FormattingSize { get; }

        protected InitializerSetTranslationBase<TInitializer> InitializerTranslations { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            _newingTranslation.WriteTo(buffer);
            InitializerTranslations.WriteTo(buffer);
        }
    }
}
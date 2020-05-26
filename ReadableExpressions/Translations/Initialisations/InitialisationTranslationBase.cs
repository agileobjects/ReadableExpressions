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
        private readonly IInitializerSetTranslation _initializerTranslations;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            IInitializerSetTranslation initializerTranslations,
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
            IInitializerSetTranslation initializerTranslations)
        {
            NodeType = initType;
            _newingTranslation = newingTranslation;
            _initializerTranslations = initializerTranslations;
            TranslationSize = newingTranslation.TranslationSize + initializerTranslations.TranslationSize;
            FormattingSize = newingTranslation.FormattingSize + initializerTranslations.FormattingSize;
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

        public int GetIndentSize()
        {
            _initializerTranslations.IsLongTranslation = TranslationSize > 40;

            return _newingTranslation.GetIndentSize() +
                   _initializerTranslations.GetIndentSize();
        }

        public int GetLineCount()
        {
            _initializerTranslations.IsLongTranslation = TranslationSize > 40;

            var lineCount = _newingTranslation.GetLineCount();

            var initializersLineCount = _initializerTranslations.GetLineCount();

            if (initializersLineCount > 1)
            {
                lineCount += initializersLineCount - 1;
            }

            return lineCount;
        }

        public void WriteTo(TranslationWriter writer)
        {
            _initializerTranslations.IsLongTranslation = TranslationSize > 40;

            _newingTranslation.WriteTo(writer);
            _initializerTranslations.WriteTo(writer);
        }
    }
}
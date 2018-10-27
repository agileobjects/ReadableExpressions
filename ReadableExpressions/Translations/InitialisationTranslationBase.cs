namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal abstract class InitialisationTranslationBase<TInitializer> : ITranslation
    {
        private readonly NewingTranslation _newingTranslation;
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            IList<TInitializer> initializers,
            Func<TInitializer, ITranslationContext, ITranslatable> initializerTranslationFactory,
            ITranslationContext context)
        {
            NodeType = initType;
            _newingTranslation = new NewingTranslation(newing, context);
            InitializerCount = initializers.Count;
            HasInitializers = InitializerCount != 0;

            if (HasNoInitializers)
            {
                _initializerTranslations = Enumerable<ITranslatable>.EmptyArray;
                EstimatedSize = _newingTranslation.EstimatedSize;
                return;
            }

            _newingTranslation = _newingTranslation.WithoutParenthesesIfParameterless();

            var estimatedSize = _newingTranslation.EstimatedSize;
            _initializerTranslations = new ITranslatable[InitializerCount];

            for (var i = 0; i < InitializerCount; ++i)
            {
                var initializerTranslation = initializerTranslationFactory.Invoke(initializers[i], context);
                _initializerTranslations[i] = initializerTranslation;
                estimatedSize += initializerTranslation.EstimatedSize;
            }

            EstimatedSize = estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        private int InitializerCount { get; }

        protected bool HasInitializers { get; }

        protected bool HasNoInitializers => !HasInitializers;

        public void WriteTo(ITranslationContext context)
        {
            _newingTranslation.WriteTo(context);

            if (HasNoInitializers)
            {
                return;
            }

            context.WriteOpeningBraceToTranslation();

            for (var i = 0; i < InitializerCount; ++i)
            {
                _initializerTranslations[i].WriteTo(context);
                context.WriteNewLineToTranslation();
            }

            context.WriteClosingBraceToTranslation();
        }
    }
}
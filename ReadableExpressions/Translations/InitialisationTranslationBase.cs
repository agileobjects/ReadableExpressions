namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal abstract class InitialisationTranslationBase<TInitializer> : ITranslation
    {
        private readonly ITranslation _newingTranslation;
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            IList<TInitializer> initializers,
            Func<TInitializer, ITranslationContext, ITranslatable> initializerTranslationFactory,
            ITranslationContext context)
            : this(
                initType,
                NewingTranslation.For(newing, context, omitParenthesesIfParameterless: initializers.Count != 0),
                initializers,
                initializerTranslationFactory,
                context)
        {
        }

        protected InitialisationTranslationBase(
            ExpressionType initType,
            ITranslation newingTranslation,
            IList<TInitializer> initializers,
            Func<TInitializer, ITranslationContext, ITranslatable> initializerTranslationFactory,
            ITranslationContext context)
        {
            NodeType = initType;
            InitializerCount = initializers.Count;
            _newingTranslation = newingTranslation;

            var estimatedSize = _newingTranslation.EstimatedSize;
            _initializerTranslations = new ITranslatable[InitializerCount];

            for (var i = 0; ;)
            {
                var initializerTranslation = initializerTranslationFactory.Invoke(initializers[i], context);
                _initializerTranslations[i] = initializerTranslation;
                estimatedSize += initializerTranslation.EstimatedSize;

                if (++i == InitializerCount)
                {
                    break;
                }
            }

            EstimatedSize = estimatedSize;
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

        private int InitializerCount { get; }

        public void WriteTo(ITranslationContext context)
        {
            var isLongTranslation = EstimatedSize > 40;

            _newingTranslation.WriteTo(context);

            if (isLongTranslation || WriteLongTranslationsToMultipleLines)
            {
                context.WriteOpeningBraceToTranslation();
            }
            else
            {
                context.WriteToTranslation(" { ");
            }

            for (var i = 0; ;)
            {
                _initializerTranslations[i].WriteTo(context);

                if (++i == InitializerCount)
                {
                    break;
                }

                if (isLongTranslation)
                {
                    context.WriteToTranslation(',');
                    context.WriteNewLineToTranslation();
                    continue;
                }

                context.WriteToTranslation(", ");
            }

            if (isLongTranslation || WriteLongTranslationsToMultipleLines)
            {
                context.WriteClosingBraceToTranslation();
            }
            else
            {
                context.WriteToTranslation(" }");
            }
        }

        protected abstract bool WriteLongTranslationsToMultipleLines { get; }
    }
}
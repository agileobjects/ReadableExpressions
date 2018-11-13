namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
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
            InitializerSetTranslation initializerTranslations,
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
            InitializerSetTranslation initializerTranslations)
        {
            NodeType = initType;
            _newingTranslation = newingTranslation;
            InitializerTranslations = initializerTranslations;
            EstimatedSize = newingTranslation.EstimatedSize + initializerTranslations.EstimatedSize;
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

        protected InitializerSetTranslation InitializerTranslations { get; }

        public void WriteTo(ITranslationContext context)
        {
            var isLongTranslation = EstimatedSize > 40;
            var writeToMultipleLines = WriteLongTranslationsToMultipleLines || isLongTranslation;

            _newingTranslation.WriteTo(context);

            if (writeToMultipleLines)
            {
                context.WriteOpeningBraceToTranslation();
            }
            else
            {
                context.WriteToTranslation(" { ");
            }

            InitializerTranslations.WriteTo(context, isLongTranslation);

            if (writeToMultipleLines)
            {
                context.WriteClosingBraceToTranslation();
            }
            else
            {
                context.WriteToTranslation(" }");
            }
        }

        protected abstract bool WriteLongTranslationsToMultipleLines { get; }

        protected abstract class InitializerSetTranslation
        {
            private readonly IList<ITranslatable> _initializerTranslations;

            protected InitializerSetTranslation(IList<TInitializer> initializers, ITranslationContext context)
            {
                Count = initializers.Count;
                _initializerTranslations = new ITranslatable[Count];

                var estimatedSize = 0;

                for (var i = 0; ;)
                {
                    // ReSharper disable once VirtualMemberCallInConstructor
                    var initializerTranslation = GetTranslation(initializers[i], context);
                    _initializerTranslations[i] = initializerTranslation;
                    estimatedSize += initializerTranslation.EstimatedSize;

                    if (++i == Count)
                    {
                        break;
                    }
                }

                EstimatedSize = estimatedSize;
            }

            protected abstract ITranslatable GetTranslation(TInitializer initializer, ITranslationContext context);

            public int EstimatedSize { get; }

            public int Count { get; }

            public abstract bool WriteToMultipleLines { get; }

            public void WriteTo(ITranslationContext context, bool isLongTranslation)
            {
                for (var i = 0; ;)
                {
                    _initializerTranslations[i].WriteTo(context);

                    if (++i == Count)
                    {
                        break;
                    }

                    if (WriteToMultipleLines || isLongTranslation)
                    {
                        context.WriteToTranslation(',');
                        context.WriteNewLineToTranslation();
                        continue;
                    }

                    context.WriteToTranslation(", ");
                }
            }
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;

    internal abstract class InitializerSetTranslationBase<TInitializer> : ITranslatable
    {
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitializerSetTranslationBase(IList<TInitializer> initializers, ITranslationContext context)
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

        public bool IsLongTranslation { get; set; }

        public abstract bool ForceWriteToMultipleLines { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (IsLongTranslation || ForceWriteToMultipleLines)
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

                if (++i == Count)
                {
                    break;
                }

                if (IsLongTranslation || ForceWriteToMultipleLines)
                {
                    context.WriteToTranslation(',');
                    context.WriteNewLineToTranslation();
                    continue;
                }

                context.WriteToTranslation(", ");
            }

            if (IsLongTranslation || ForceWriteToMultipleLines)
            {
                context.WriteClosingBraceToTranslation();
            }
            else
            {
                context.WriteToTranslation(" }");
            }
        }
    }
}
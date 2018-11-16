namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;

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

        private bool WriteToMultipleLines => ForceWriteToMultipleLines || IsLongTranslation;

        public void WriteTo(TranslationBuffer buffer)
        {
            if (WriteToMultipleLines)
            {
                buffer.WriteOpeningBraceToTranslation();
            }
            else
            {
                buffer.WriteToTranslation(" { ");
            }

            for (var i = 0; ;)
            {
                _initializerTranslations[i].WriteTo(buffer);

                if (++i == Count)
                {
                    break;
                }

                if (WriteToMultipleLines)
                {
                    buffer.WriteToTranslation(',');
                    buffer.WriteNewLineToTranslation();
                    continue;
                }

                buffer.WriteToTranslation(", ");
            }

            if (WriteToMultipleLines)
            {
                buffer.WriteClosingBraceToTranslation();
            }
            else
            {
                buffer.WriteToTranslation(" }");
            }
        }
    }
}
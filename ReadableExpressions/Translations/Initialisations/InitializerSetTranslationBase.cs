namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;

    internal abstract class InitializerSetTranslationBase<TInitializer> : IInitializerSetTranslation
    {
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitializerSetTranslationBase(IList<TInitializer> initializers, ITranslationContext context)
        {
            var initializersCount = initializers.Count;
            Count = initializersCount;
            _initializerTranslations = new ITranslatable[initializersCount];

            var translationSize = 0;
            var formattingSize = 0;

            for (var i = 0; ;)
            {
                // ReSharper disable once VirtualMemberCallInConstructor
                var initializerTranslation = GetTranslation(initializers[i], context);
                _initializerTranslations[i] = initializerTranslation;
                translationSize += initializerTranslation.TranslationSize;
                formattingSize += initializerTranslation.FormattingSize;

                ++i;

                if (i == initializersCount)
                {
                    break;
                }
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        protected abstract ITranslatable GetTranslation(TInitializer initializer, ITranslationContext context);

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int Count { get; }

        public bool IsLongTranslation { get; set; }

        public abstract bool ForceWriteToMultipleLines { get; }

        private bool WriteToMultipleLines => ForceWriteToMultipleLines || IsLongTranslation;

        public int GetLineCount()
        {
            return
                (WriteToMultipleLines ? 1 : 0) +
                _initializerTranslations.GetLineCount(Count);
        }

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

                ++i;

                if (i == Count)
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
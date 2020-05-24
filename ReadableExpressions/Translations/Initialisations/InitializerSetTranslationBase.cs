namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;
    using static Constants;

    internal abstract class InitializerSetTranslationBase<TInitializer> : IInitializerSetTranslation
    {
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitializerSetTranslationBase(IList<TInitializer> initializers, ITranslationContext context)
        {
            var initializersCount = initializers.Count;
            Count = initializersCount;
            _initializerTranslations = new ITranslatable[initializersCount];

            var translationSize = 4;
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

                translationSize += 2; // For ', '
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

        public int GetIndentSize()
        {
            var writeToMultipleLines = WriteToMultipleLines;
            var indentSize = writeToMultipleLines ? 0 : 2;

            for (var i = 0; ;)
            {
                var initializerTranslation = _initializerTranslations[i];
                var initializerIndentSize = initializerTranslation.GetIndentSize();

                if (writeToMultipleLines)
                {
                    initializerIndentSize += initializerTranslation.GetLineCount() * IndentLength;
                }

                indentSize += initializerIndentSize;

                ++i;

                if (i == Count)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
            => WriteToMultipleLines ? GetMultiLineCount() : _initializerTranslations.GetLineCount(Count);

        private int GetMultiLineCount()
        {
            var lineCount = 2; // for { and }

            for (var i = 0; ;)
            {
                lineCount += _initializerTranslations[i].GetLineCount();

                ++i;

                if (i == Count)
                {
                    return lineCount;
                }
            }
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (WriteToMultipleLines)
            {
                buffer.WriteOpeningBraceToTranslation();
            }
            else
            {
                if (!buffer.TranslationQuery(q => q.TranslationEndsWith(' ')))
                {
                    buffer.WriteSpaceToTranslation();
                }

                buffer.WriteToTranslation("{ ");
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
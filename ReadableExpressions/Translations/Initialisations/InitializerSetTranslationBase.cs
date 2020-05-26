namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;

    internal abstract class InitializerSetTranslationBase<TInitializer> : IInitializerSetTranslation
    {
        private readonly TranslationSettings _settings;
        private readonly IList<ITranslatable> _initializerTranslations;

        protected InitializerSetTranslationBase(IList<TInitializer> initializers, ITranslationContext context)
        {
            _settings = context.Settings;

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
            var indentLength = _settings.IndentLength;

            for (var i = 0; ;)
            {
                var initializerTranslation = _initializerTranslations[i];
                var initializerIndentSize = initializerTranslation.GetIndentSize();

                if (writeToMultipleLines)
                {
                    initializerIndentSize += initializerTranslation.GetLineCount() * indentLength;
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

        public void WriteTo(TranslationWriter writer)
        {
            if (WriteToMultipleLines)
            {
                writer.WriteOpeningBraceToTranslation();
            }
            else
            {
                if (!writer.TranslationQuery(q => q.TranslationEndsWith(' ')))
                {
                    writer.WriteSpaceToTranslation();
                }

                writer.WriteToTranslation("{ ");
            }

            for (var i = 0; ;)
            {
                _initializerTranslations[i].WriteTo(writer);

                ++i;

                if (i == Count)
                {
                    break;
                }

                if (WriteToMultipleLines)
                {
                    writer.WriteToTranslation(',');
                    writer.WriteNewLineToTranslation();
                    continue;
                }

                writer.WriteToTranslation(", ");
            }

            if (WriteToMultipleLines)
            {
                writer.WriteClosingBraceToTranslation();
            }
            else
            {
                writer.WriteToTranslation(" }");
            }
        }
    }
}
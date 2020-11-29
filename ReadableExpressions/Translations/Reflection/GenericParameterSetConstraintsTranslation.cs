namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.Generic;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a set of generic parameter constraints.
    /// </summary>
    public class GenericParameterSetConstraintsTranslation : ITranslatable
    {
        private readonly TranslationSettings _settings;
        private readonly ITranslatable[] _constraintTranslations;
        private readonly int _constraintsCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterSetConstraintsTranslation"/>
        /// class.
        /// </summary>
        /// <param name="genericParameters">The <see cref="IGenericParameter"/>s to write to the translation.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public GenericParameterSetConstraintsTranslation(
            IList<IGenericParameter> genericParameters,
            TranslationSettings settings)
        {
            _settings = settings;

            var genericArgumentCount = genericParameters.Count;
            var translationSize = 0;
            var formattingSize = 0;

            _constraintTranslations = new ITranslatable[genericArgumentCount];

            for (var i = 0; ;)
            {
                var parameter = genericParameters[i];
                var constraintsTranslation = GenericConstraintsTranslation.For(parameter, settings);

                translationSize += constraintsTranslation.TranslationSize;
                formattingSize += constraintsTranslation.FormattingSize;

                _constraintTranslations[i] = constraintsTranslation;

                if (constraintsTranslation.TranslationSize > 0)
                {
                    ++_constraintsCount;
                }

                ++i;

                if (i == genericArgumentCount)
                {
                    break;
                }
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize()
            => _constraintsCount * _settings.Indent.Length;

        /// <inheritdoc />
        public int GetLineCount() => _constraintsCount + 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            if (_constraintsCount == 0)
            {
                return;
            }

            writer.WriteNewLineToTranslation();
            writer.Indent();

            for (var i = 0; ;)
            {
                _constraintTranslations[i].WriteTo(writer);
                ++i;

                if (i == _constraintsCount)
                {
                    break;
                }

                writer.WriteNewLineToTranslation();
            }

            writer.Unindent();
        }
    }
}
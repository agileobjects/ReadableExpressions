namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.Generic;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a set of generic parameters.
    /// </summary>
    public class GenericParameterSetDefinitionTranslation : ITranslatable
    {
        private readonly int _genericParameterCount;
        private readonly ITranslatable[] _genericParameterTranslations;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterSetDefinitionTranslation"/>
        /// class.
        /// </summary>
        /// <param name="genericParameters">The <see cref="IGenericParameter"/>s to write to the translation.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public GenericParameterSetDefinitionTranslation(
            IList<IGenericParameter> genericParameters,
            TranslationSettings settings)
        {
            _genericParameterCount = genericParameters.Count;

            if (_genericParameterCount == 0)
            {
                return;
            }

            var translationSize = 2; // For <> brackets
            var formattingSize = 0;

            _genericParameterTranslations = new ITranslatable[_genericParameterCount];

            for (var i = 0; ;)
            {
                var argument = genericParameters[i];

                var argumentTranslation = new TypeNameTranslation(argument, settings);

                translationSize += argumentTranslation.TranslationSize;
                formattingSize += argumentTranslation.FormattingSize;

                _genericParameterTranslations[i] = argumentTranslation;

                ++i;

                if (i == _genericParameterCount)
                {
                    break;
                }

                translationSize += ", ".Length;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => 0;

        /// <inheritdoc />
        public int GetLineCount() => 0;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            if (_genericParameterCount == 0)
            {
                return;
            }

            writer.WriteToTranslation('<');

            for (var i = 0; ;)
            {
                _genericParameterTranslations[i].WriteTo(writer);

                ++i;

                if (i == _genericParameterCount)
                {
                    break;
                }

                writer.WriteToTranslation(", ");
            }

            writer.WriteToTranslation('>');
        }
    }
}
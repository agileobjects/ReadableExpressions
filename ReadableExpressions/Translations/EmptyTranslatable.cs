namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// A Null Object <see cref="ITranslatable"/> implementation.
    /// </summary>
    public class EmptyTranslatable : IPotentialEmptyTranslatable
    {
        /// <summary>
        /// Gets the singleton <see cref="EmptyTranslatable"/> instance.
        /// </summary>
        public static readonly IPotentialEmptyTranslatable Instance = new EmptyTranslatable();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyTranslatable"/> class.
        /// </summary>
        protected EmptyTranslatable()
        {
        }

        /// <inheritdoc />
        public int TranslationSize => 0;

        /// <inheritdoc />
        public int FormattingSize => 0;

        /// <inheritdoc />
        public bool IsEmpty => true;

        /// <inheritdoc />
        public int GetIndentSize() => 0;

        /// <inheritdoc />
        public int GetLineCount() => 0;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    internal class NullTranslatable : ITranslatable
    {
        public static readonly ITranslatable Instance = new NullTranslatable();

        public int TranslationSize => 0;

        public int FormattingSize => 0;

        public int GetIndentSize() => 0;

        public int GetLineCount() => 0;

        public void WriteTo(TranslationWriter writer)
        {
        }
    }
}
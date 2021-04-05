namespace AgileObjects.ReadableExpressions.Translations
{
    internal interface ITranslationQuery
    {
        bool TranslationEndsWith(char character);
        
        bool TranslationEndsWith(string substring);
        
        bool TranslationEndsWithBlankLine();
    }
}
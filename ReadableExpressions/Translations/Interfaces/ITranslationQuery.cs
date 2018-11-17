namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    internal interface ITranslationQuery
    {
        bool TranslationEndsWith(char character);
        
        bool TranslationEndsWith(string substring);
        
        bool TranslationEndsWithBlankLine();
    }
}
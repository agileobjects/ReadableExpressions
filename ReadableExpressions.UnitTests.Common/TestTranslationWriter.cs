namespace AgileObjects.ReadableExpressions.UnitTests.Common
{
    using Translations;

    public class TestTranslationWriter : TranslationWriter
    {
        public TestTranslationWriter(ITranslatable translatable) 
            : base(TestTranslationSettings.TestSettings, translatable)
        {
        }
    }
}
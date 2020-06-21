namespace AgileObjects.ReadableExpressions
{
    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    public class TranslationSettings : TranslationSettingsBase<TranslationSettings>
    {
        internal static readonly TranslationSettings Default = new TranslationSettings();

        internal override TranslationSettings Settings => this;
    }
}

namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System.Text.RegularExpressions;
    using Configuration;
    using Theming;
    using static System.Text.RegularExpressions.RegexOptions;

    public class TranslationViewModel
    {
        private string _translation;
        private string _translationRaw;

        public string VisualizerTitle
            => "ReadableExpressions v" + VersionNumber.FileVersion;

        public VisualizerDialogSettings Settings => VisualizerDialogSettings.Instance;

        public VisualizerDialogTheme Theme
        {
            get => Settings.Theme;
            private set => Settings.Theme = value;
        }

        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                TranslationRaw = null;
            }
        }

        public string TranslationRaw
        {
            get => _translationRaw ??= GetRawTranslation();
            private set => _translationRaw = value;
        }

        private static readonly Regex _htmlMatcher = new Regex("<[^>]+>", Compiled);

        private string GetRawTranslation()
        {
            return _htmlMatcher
                .Replace(Translation, string.Empty)
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");
        }
    }
}

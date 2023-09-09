namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using Theming;

    public class VisualizerDialogSettings
    {
        public static readonly VisualizerDialogSettings Instance;

        private static readonly VisualizerDialogSettings _default;

        static VisualizerDialogSettings()
        {
            _default = new VisualizerDialogSettings
            {
                Theme = VisualizerDialogTheme.Light,
                Font = VisualizerDialogFontSettings.Monospace,
                Size = new VisualizerDialogSizeSettings()
            };

            Instance = GetInstance();
        }

        public static VisualizerDialogSettings GetInstance()
            => VisualizerDialogSettingsManager.TryLoad(out var settings) ? settings : _default;

        public VisualizerDialogTheme Theme { get; set; }

        public VisualizerDialogFontSettings Font { get; set; }

        public VisualizerDialogSizeSettings Size { get; set; }

        public string Indent { get; set; } = "    ";

        public bool UseFullyQualifiedTypeNames { get; set; }

        public bool UseExplicitTypeNames { get; set; }

        public bool UseExplicitGenericParameters { get; set; }

        public bool DeclareOutputParametersInline { get; set; }

        public bool DiscardUnusedParameters { get; set; }

        public bool ShowImplicitArrayTypes { get; set; }

        public bool ShowLambdaParameterTypeNames { get; set; }

        public bool ShowQuotedLambdaComments { get; set; }

        public bool ShowStringConcatCalls { get; set; }

        public void Save() => VisualizerDialogSettingsManager.Save(this);
    }
}
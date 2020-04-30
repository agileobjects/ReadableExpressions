namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using Theming;

    internal class VisualizerDialogSettings
    {
        private static readonly VisualizerDialogSettings _default = new VisualizerDialogSettings
        {
            Theme = ExpressionTranslationTheme.Light
        };

        public static readonly VisualizerDialogSettings Instance = GetInstance();

        public static VisualizerDialogSettings GetInstance()
            => VisualizerDialogSettingsManager.TryLoad(out var settings) ? settings : _default;

        public ExpressionTranslationTheme Theme { get; set; }

        public bool UseFullyQualifiedTypeNames { get; set; }

        public bool UseExplicitTypeNames { get; set; }

        public bool UseExplicitGenericParameters { get; set; }

        public bool DeclareOutputParametersInline { get; set; }

        public bool ShowLambdaParameterTypeNames { get; set; }

        public bool ShowQuotedLambdaComments { get; set; }

        public TranslationSettings Update(TranslationSettings settings)
        {
            if (UseFullyQualifiedTypeNames)
            {
                settings = settings.UseFullyQualifiedTypeNames;
            }

            if (UseExplicitTypeNames)
            {
                settings = settings.UseExplicitTypeNames;
            }

            if (UseExplicitGenericParameters)
            {
                settings = settings.UseExplicitGenericParameters;
            }

            if (DeclareOutputParametersInline)
            {
                settings = settings.DeclareOutputParametersInline;
            }

            if (ShowLambdaParameterTypeNames)
            {
                settings = settings.ShowLambdaParameterTypes;
            }

            if (ShowQuotedLambdaComments)
            {
                settings = settings.ShowQuotedLambdaComments;
            }

            return settings;
        }

        public void Save() => VisualizerDialogSettingsManager.Save(this);
    }
}
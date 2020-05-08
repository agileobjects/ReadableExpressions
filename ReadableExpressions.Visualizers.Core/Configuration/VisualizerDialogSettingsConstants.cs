namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using Theming;

    internal static class VisualizerDialogSettingsConstants
    {
        private const string _theme = "Theme";
        public const string ThemeName = _theme + "." + nameof(VisualizerDialogTheme.Name);
        public const string ThemeBackground = _theme + "." + nameof(VisualizerDialogTheme.Background);
        public const string ThemeDefault = _theme + "." + nameof(VisualizerDialogTheme.Default);
        public const string ThemeToolbar = _theme + "." + nameof(VisualizerDialogTheme.Toolbar);
        public const string ThemeMenu = _theme + "." + nameof(VisualizerDialogTheme.Menu);
        public const string ThemeKeyword = _theme + "." + nameof(VisualizerDialogTheme.Keyword);
        public const string ThemeVariable = _theme + "." + nameof(VisualizerDialogTheme.Variable);
        public const string ThemeTypeName = _theme + "." + nameof(VisualizerDialogTheme.TypeName);
        public const string ThemeInterfaceName = _theme + "." + nameof(VisualizerDialogTheme.InterfaceName);
        public const string ThemeCommandStatement = _theme + "." + nameof(VisualizerDialogTheme.CommandStatement);
        public const string ThemeText = _theme + "." + nameof(VisualizerDialogTheme.Text);
        public const string ThemeNumeric = _theme + "." + nameof(VisualizerDialogTheme.Numeric);
        public const string ThemeMethodName = _theme + "." + nameof(VisualizerDialogTheme.MethodName);
        public const string ThemeComment = _theme + "." + nameof(VisualizerDialogTheme.Comment);
       
        public const string FontName = "Font." + nameof(VisualizerDialogFont.Name);
        public const string FontSize = "Font." + nameof(VisualizerDialogFont.Size);
    }
}
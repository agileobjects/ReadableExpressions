namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using Theming;

    internal static class VisualizerDialogSettingsConstants
    {
        private const string _theme = "Theme";
        public const string ThemeName = _theme + "." + nameof(ExpressionTranslationTheme.Name);
        public const string ThemeBackground = _theme + "." + nameof(ExpressionTranslationTheme.Background);
        public const string ThemeDefault = _theme + "." + nameof(ExpressionTranslationTheme.Default);
        public const string ThemeToolbar = _theme + "." + nameof(ExpressionTranslationTheme.Toolbar);
        public const string ThemeMenu = _theme + "." + nameof(ExpressionTranslationTheme.Menu);
        public const string ThemeKeyword = _theme + "." + nameof(ExpressionTranslationTheme.Keyword);
        public const string ThemeVariable = _theme + "." + nameof(ExpressionTranslationTheme.Variable);
        public const string ThemeTypeName = _theme + "." + nameof(ExpressionTranslationTheme.TypeName);
        public const string ThemeInterfaceName = _theme + "." + nameof(ExpressionTranslationTheme.InterfaceName);
        public const string ThemeCommandStatement = _theme + "." + nameof(ExpressionTranslationTheme.CommandStatement);
        public const string ThemeText = _theme + "." + nameof(ExpressionTranslationTheme.Text);
        public const string ThemeNumeric = _theme + "." + nameof(ExpressionTranslationTheme.Numeric);
        public const string ThemeMethodName = _theme + "." + nameof(ExpressionTranslationTheme.MethodName);
        public const string ThemeComment = _theme + "." + nameof(ExpressionTranslationTheme.Comment);
    }
}
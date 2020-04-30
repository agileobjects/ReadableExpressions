namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using static DialogConstants;
    using static Theming.ExpressionTranslationTheme;

    internal class ThemeSelector : MenuItemPanelBase
    {
        public ThemeSelector(VisualizerDialog dialog)
            : base(dialog)
        {
            Controls.Add(new MenuItemLabel("Theme", OptionControlWidth * 2, dialog));
            Controls.Add(new ThemeOption(Light, dialog));
            Controls.Add(new ThemeOption(Dark, dialog));
        }
    }
}
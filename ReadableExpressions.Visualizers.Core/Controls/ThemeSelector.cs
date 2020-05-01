namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using static DialogConstants;
    using static Theming.ExpressionTranslationTheme;

    internal class ThemeSelector : MenuItemPanelBase
    {
        public ThemeSelector(VisualizerDialog dialog)
            : base(dialog)
        {
            var label = new MenuItemLabel(
                "Theme",
                "Set the visualizer theme",
                ThemeOptionWidth * 2,
                dialog);

            Controls.Add(label);
            Controls.Add(new ThemeOption(Light, dialog));
            Controls.Add(new ThemeOption(Dark, dialog));
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;
    using static DialogConstants;
    using static Theming.ExpressionTranslationTheme;

    internal class ThemeSelector : FlowLayoutPanel
    {
        public ThemeSelector(VisualizerDialog dialog)
        {
            FlowDirection = FlowDirection.LeftToRight;
            Width = MenuWidth;

            dialog.RegisterThemeable(this);

            Controls.Add(new MenuItemLabel("Theme", OptionControlWidth * 2, dialog));
            Controls.Add(new ThemeOption(Light, dialog));
            Controls.Add(new ThemeOption(Dark, dialog));
        }
    }
}
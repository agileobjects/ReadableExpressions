namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Theming
{
    using System.Windows.Forms;
    using Core.Theming;

    internal static class ThemingExtensions
    {
        public static void ApplyTo(this VisualizerDialogTheme theme, Control control)
        {
            if (control is ToolStrip toolStrip && !(toolStrip is ToolStripDropDown))
            {
                theme.ApplyTo(toolStrip);
                return;
            }

            control.BackColor = theme.MenuColour;
            control.ForeColor = theme.ForeColour;

            if (control is IThemeable themeable)
            {
                themeable.Apply(theme);
            }
        }

        public static void ApplyTo(this VisualizerDialogTheme theme, ToolStrip toolStrip)
        {
            toolStrip.BackColor = theme.ToolbarColour;
            toolStrip.ForeColor = theme.ForeColour;
        }
    }
}

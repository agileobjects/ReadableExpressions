namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Theming
{
    using System.Windows.Forms;

    internal static class ThemingExtensions
    {
        public static void ApplyTo(
            this VisualizerDialogColourTable colourTable, 
            Control control)
        {
            if (control is ToolStrip toolStrip && !(toolStrip is ToolStripDropDown))
            {
                colourTable.ApplyTo(toolStrip);
                return;
            }

            control.BackColor = colourTable.MenuColour;
            control.ForeColor = colourTable.ForeColour;

            if (control is IThemeable themeable)
            {
                themeable.Apply(colourTable);
            }
        }

        public static void ApplyTo(
            this VisualizerDialogColourTable colourTable, 
            ToolStrip toolStrip)
        {
            toolStrip.BackColor = colourTable.ToolbarColour;
            toolStrip.ForeColor = colourTable.ForeColour;
        }
    }
}

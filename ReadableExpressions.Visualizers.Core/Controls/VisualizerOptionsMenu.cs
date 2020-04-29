namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;

    internal class VisualizerOptionsMenu : ToolStripDropDown
    {
        public VisualizerOptionsMenu(VisualizerDialog dialog)
        {
            BackColor = dialog.Theme.MenuColour;
            ForeColor = dialog.Theme.ForeColour;
            Width = DialogConstants.MenuWidth;

            base.Items.Add(new ToolStripControlHost(new ThemeSelectorPanel(dialog)));
        }
    }
}
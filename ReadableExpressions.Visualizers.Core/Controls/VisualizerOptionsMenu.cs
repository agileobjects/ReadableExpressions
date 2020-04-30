namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;

    internal class VisualizerOptionsMenu : ToolStripDropDown
    {
        public VisualizerOptionsMenu(VisualizerDialog dialog)
        {
            dialog.RegisterThemeable(this);

            Width = DialogConstants.MenuWidth;

            base.Items.Add(new ToolStripControlHost(new ThemeSelector(dialog)));
        }
    }
}
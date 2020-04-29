namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;

    internal class VisualizerOptionsMenuItem : ToolStripMenuItem
    {
        public VisualizerOptionsMenuItem(VisualizerDialog dialog)
            : base("Options")
        {
            DropDown = new VisualizerOptionsMenu(dialog);
        }
    }
}
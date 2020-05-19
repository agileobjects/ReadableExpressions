namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;

    internal class VisualizerViewerPanel : Panel
    {
        public VisualizerViewerPanel(VisualizerDialog dialog)
        {
            base.Dock = DockStyle.Fill;
            base.AutoSize = true;

            dialog.RegisterThemeable(this);

            Viewer = new VisualizerViewer(this, dialog);

            Controls.Add(Viewer);
        }

        public VisualizerViewer Viewer { get; }
    }
}
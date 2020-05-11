namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class VisualizerDialogRenderer : ToolStripProfessionalRenderer
    {
        private readonly VisualizerDialog _dialog;

        public VisualizerDialogRenderer(VisualizerDialog dialog)
            : base(new VisualizerDialogColourTable(dialog))
        {
            _dialog = dialog;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var backgroundBrush = e.Item.Selected && e.Item.Text == "Options"
                ? _dialog.Theme.MenuColourBrush
                : _dialog.Theme.ToolbarColourBrush;

            e.Graphics.FillRectangle(
                backgroundBrush,
                new Rectangle(Point.Empty, e.Item.Size));
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            e.Graphics.FillRectangle(
                _dialog.Theme.MenuColourBrush,
                new Rectangle(Point.Empty, e.Item.Size));
        }
    }
}
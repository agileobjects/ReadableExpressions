namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class ExpressionDialogRenderer : ToolStripProfessionalRenderer
    {
        private readonly VisualizerDialog _dialog;

        public ExpressionDialogRenderer(VisualizerDialog dialog)
            : base(new ExpressionDialogColourTable(dialog))
        {
            _dialog = dialog;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var backgroundColor = e.Item.Selected && e.Item.Text == "Options"
                ? _dialog.Theme.MenuColour
                : _dialog.Theme.ToolbarColour;

            e.Graphics.FillRectangle(
                new SolidBrush(backgroundColor),
                new Rectangle(Point.Empty, e.Item.Size));
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            e.Graphics.FillRectangle(
                new SolidBrush(_dialog.Theme.MenuColour),
                new Rectangle(Point.Empty, e.Item.Size));
        }
    }
}
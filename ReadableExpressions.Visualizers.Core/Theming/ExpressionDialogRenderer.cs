namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class ExpressionDialogRenderer : ToolStripProfessionalRenderer
    {
        private readonly ExpressionTranslationTheme _theme;

        public ExpressionDialogRenderer(ExpressionTranslationTheme theme)
            : base(new ExpressionDialogColourTable(theme))
        {
            _theme = theme;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var backgroundColor = e.Item.Selected && e.Item.Text == "Options"
                ? _theme.MenuColour
                : _theme.ToolbarColour;

            e.Graphics.FillRectangle(
                new SolidBrush(backgroundColor),
                new Rectangle(Point.Empty, e.Item.Size));
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            e.Graphics.FillRectangle(
                new SolidBrush(_theme.MenuColour),
                new Rectangle(Point.Empty, e.Item.Size));
        }
    }
}
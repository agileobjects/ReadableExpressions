namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class MenuItemLabel : Label
    {
        private static readonly Font _font = new Font(new FontFamily("Arial"), 10);

        public MenuItemLabel(
            string text,
            string tooltip,
            int controlWidth,
            VisualizerDialog dialog)
        {
            Size = new Size(MenuWidth - controlWidth, MenuItemHeight);

            dialog.RegisterThemeable(this);

            base.Font = _font;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = text;

            dialog.ToolTip.SetToolTip(this, tooltip);
        }
    }
}
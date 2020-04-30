namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class MenuItemLabel : Label
    {
        public MenuItemLabel(
            string text,
            int controlWidth,
            VisualizerDialog dialog)
        {
            Size = new Size(MenuWidth - controlWidth, MenuItemHeight);

            dialog.RegisterThemeable(this);

            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = text;
        }
    }
}
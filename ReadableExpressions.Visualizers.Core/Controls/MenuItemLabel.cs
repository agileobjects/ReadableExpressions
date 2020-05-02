namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class MenuItemLabel : Label
    {
        public MenuItemLabel(
            string text,
            string tooltip,
            int controlWidth,
            VisualizerDialog dialog)
        {
            Size = new Size(MenuWidth - controlWidth, MenuItemHeight);

            dialog.RegisterThemeable(this);

            base.Font = MenuItemFont;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = text;

            dialog.ToolTip.SetToolTip(this, tooltip);
        }
    }
}
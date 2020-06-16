namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using static Core.DialogConstants;

    internal class MenuItemLabel : Label
    {
        public MenuItemLabel(
            string text,
            string tooltip,
            int controlWidth,
            VisualizerDialog dialog)
        {
            Size = new SizeF(
                MenuWidth * dialog.WidthFactor - controlWidth,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            dialog.RegisterThemeable(this);

            base.Font = MenuItemFont;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = text;

            dialog.ToolTip.SetToolTip(this, tooltip);
        }
    }
}
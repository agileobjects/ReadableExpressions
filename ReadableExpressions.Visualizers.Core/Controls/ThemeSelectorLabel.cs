namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class ThemeSelectorLabel : Label
    {
        public ThemeSelectorLabel(VisualizerDialog dialog)
        {
            Size = new Size(MenuWidth - ThemeSelectorWidth, MenuItemHeight);

            base.BackColor = dialog.Theme.MenuColour;
            base.ForeColor = dialog.Theme.ForeColour;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = "Theme";
        }
    }
}
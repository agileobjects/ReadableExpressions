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

            dialog.RegisterThemeable(this);

            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = "Theme";
        }
    }
}
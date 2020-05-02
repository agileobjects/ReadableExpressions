namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;

    internal abstract class MenuItemPanelBase : FlowLayoutPanel
    {
        protected MenuItemPanelBase(VisualizerDialog dialog)
        {
            FlowDirection = FlowDirection.LeftToRight;
            Width = DialogConstants.MenuWidth;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            dialog.RegisterThemeable(this);
        }
    }
}
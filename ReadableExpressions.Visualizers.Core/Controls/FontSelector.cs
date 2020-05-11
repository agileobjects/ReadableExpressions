namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class FontSelector : MenuItemPanelBase
    {
        public FontSelector(ILazyMenu menu, VisualizerDialog dialog)
            : base(dialog)
        {
            var fontFamilySelector = new FontFamilySelector(menu, dialog);
            var fontSizeSelector = new FontSizeSelector(dialog);

            var label = new MenuItemLabel(
                "Code sample font",
                "Set the visualizer code font",
                fontFamilySelector.Width + fontSizeSelector.Width,
                dialog);

            Controls.Add(label);
            Controls.Add(fontFamilySelector);
            Controls.Add(fontSizeSelector);
        }
    }
}
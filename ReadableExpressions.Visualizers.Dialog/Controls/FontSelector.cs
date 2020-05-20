namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class FontSelector : MenuItemPanelBase
    {
        public FontSelector(VisualizerDialog dialog)
            : base(dialog)
        {
            var fontFamilySelector = new FontFamilySelector(dialog);
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
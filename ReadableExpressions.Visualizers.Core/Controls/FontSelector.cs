namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class FontSelector : MenuItemPanelBase
    {
        public FontSelector(ILazyMenu menu, VisualizerDialog dialog)
            : base(dialog)
        {
            var label = new MenuItemLabel(
                "Code sample font",
                "Set the visualizer code font",
                DialogConstants.FontSelectorWidth,
                dialog);

            Controls.Add(label);
            Controls.Add(new FontFamilySelector(menu, dialog));
        }
    }
}
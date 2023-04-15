namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class IndentSelector : MenuItemPanelBase
    {
        public IndentSelector(VisualizerDialog dialog)
            : base(dialog)
        {
            var indentLengthSelector = new IndentLengthSelector(dialog);
            var indentTypeSelector = new IndentTypeSelector(dialog);

            var label = new MenuItemLabel(
                "Indent using",
                "Set the visualizer code indent",
                indentLengthSelector.Width + indentTypeSelector.Width,
                dialog);

            Controls.Add(label);
            Controls.Add(indentLengthSelector);
            Controls.Add(indentTypeSelector);
        }
    }
}

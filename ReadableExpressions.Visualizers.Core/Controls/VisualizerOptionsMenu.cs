namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;
    using static DialogConstants;

    internal class VisualizerOptionsMenu : ToolStripDropDown
    {
        public VisualizerOptionsMenu(VisualizerDialog dialog)
        {
            Width = MenuWidth;

            dialog.RegisterThemeable(this);

            base.Items.Add(new ToolStripControlHost(new ThemeSelector(dialog)));
            base.Items.Add(new ToolStripControlHost(new FullyQualifiedTypeNamesOption(dialog)));
            base.Items.Add(new ToolStripControlHost(new ExplicitGenericParamsOption(dialog)));
            base.Items.Add(new ToolStripControlHost(new DeclareOutParamsInlineOption(dialog)));
            base.Items.Add(new ToolStripControlHost(new QuotedLambdaCommentsOption(dialog)));
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class ExpressionDialogColourTable : ProfessionalColorTable
    {
        private readonly VisualizerDialog _dialog;

        public ExpressionDialogColourTable(VisualizerDialog dialog)
        {
            _dialog = dialog;
        }

        public override Color ToolStripBorder => _dialog.Theme.ForeColour;

        public override Color ToolStripDropDownBackground => _dialog.Theme.MenuColour;
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class ExpressionDialogColourTable : ProfessionalColorTable
    {
        private readonly ExpressionTranslationTheme _theme;

        public ExpressionDialogColourTable(ExpressionTranslationTheme theme)
        {
            _theme = theme;
        }

        public override Color ToolStripBorder => _theme.ForeColour;

        public override Color ToolStripDropDownBackground => _theme.MenuColour;
    }
}
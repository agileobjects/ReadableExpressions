namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Theming
{
    using System.Drawing;
    using System.Windows.Forms;
    using Core.Theming;

    internal class VisualizerDialogColourTable : ProfessionalColorTable
    {
        private readonly VisualizerDialog _dialog;
        private Color? _foreColour;
        private Brush _foreColourBrush;
        private Brush _foreLowlightColourBrush;
        private Color? _toolbarColour;
        private Brush _toolbarColourBrush;
        private Color? _menuColour;
        private Brush _menuColourBrush;
        private Color? _menuHighlightColour;
        private Brush _menuHighlightColourBrush;

        public VisualizerDialogColourTable(VisualizerDialog dialog)
        {
            _dialog = dialog;
        }

        private VisualizerDialogTheme Theme => _dialog.Theme;

        public void HandleThemeChanged()
        {
            _foreColour = null;
            _foreColourBrush = null;
            _foreLowlightColourBrush = null;
            _toolbarColour = null;
            _toolbarColourBrush = null;
            _menuColour = null;
            _menuColourBrush = null;
            _menuHighlightColour = null;
            _menuHighlightColourBrush = null;
        }

        public override Color ToolStripBorder => ForeColour;

        public override Color ToolStripDropDownBackground => MenuColour;

        public Color ForeColour
            => _foreColour ??= ColorTranslator.FromHtml(Theme.Default);

        public Brush ForeColourBrush
            => _foreColourBrush ??= new SolidBrush(ForeColour);

        public Brush ForeLowlightColourBrush
            => _foreLowlightColourBrush ??= new SolidBrush(
                ForeColour.ChangeBrightness(ForeColour.IsDark() ? 0.5f : -0.4f));

        public Color ToolbarColour
            => _toolbarColour ??= ColorTranslator.FromHtml(Theme.Toolbar);

        public Brush ToolbarColourBrush
            => _toolbarColourBrush ??= new SolidBrush(ToolbarColour);

        public Color MenuColour
            => _menuColour ??= ColorTranslator.FromHtml(Theme.Menu);

        public Brush MenuColourBrush
            => _menuColourBrush ??= new SolidBrush(MenuColour);

        public Color MenuHighlightColour
            => _menuHighlightColour ??= ColorTranslator.FromHtml(Theme.MenuHighlight);

        public Brush MenuHighlightColourBrush
            => _menuHighlightColourBrush ??= new SolidBrush(MenuHighlightColour);

        public string IconSuffix
            => MenuColour.IsDark() ? "Dark" : null;
    }
}
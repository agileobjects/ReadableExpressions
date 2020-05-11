namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;
    using System.Windows.Forms;

    internal class VisualizerDialogTheme
    {
        public static readonly VisualizerDialogTheme Dark = new VisualizerDialogTheme
        {
            Name = "Dark",
            Background = "#1E1E1E",
            Default = "#DCDCDC",
            Toolbar = "#2D2D30",
            Menu = "#1B1B1C",
            MenuHighlight = "#333334",
            Keyword = "#569CD6",
            Variable = "#9CDCFE",
            TypeName = "#4EC9B0",
            InterfaceName = "#B8D7A3",
            CommandStatement = "#D8A0DF",
            Text = "#D69D85",
            Numeric = "#B5CEA8",
            MethodName = "#DCDCAA",
            Comment = "#57A64A"
        };

        public static readonly VisualizerDialogTheme Light = new VisualizerDialogTheme
        {
            Name = "Light",
            Background = "#FFF",
            Default = "#000",
            Toolbar = "#EEEEF2",
            Menu = "#F6F6F6",
            MenuHighlight = "#C9DEF5",
            Keyword = "#0000FF",
            Variable = "#1F377F",
            TypeName = "#2B91AF",
            InterfaceName = "#2B91AF",
            CommandStatement = "#8F08C4",
            Text = "#A31515",
            Numeric = "#000",
            MethodName = "#74531F",
            Comment = "#008000"
        };

        private Color? _foreColour;
        private Brush _foreColourBrush;
        private Brush _foreLowlightColourBrush;
        private Color? _toolbarColour;
        private Brush _toolbarColourBrush;
        private Color? _menuColour;
        private Brush _menuColourBrush;
        private Color? _menuHighlightColour;
        private Brush _menuHighlightColourBrush;

        public string Name { get; set; }

        public string Background { get; set; }

        public string Default { get; set; }

        public Color ForeColour
            => _foreColour ??= ColorTranslator.FromHtml(Default);

        public Brush ForeColourBrush
            => _foreColourBrush ??= new SolidBrush(ForeColour);

        public Brush ForeLowlightColourBrush
            => _foreLowlightColourBrush ??= new SolidBrush(
                ForeColour.ChangeBrightness(ForeColour.IsDark() ? 0.5f : -0.4f));

        public string Toolbar { get; set; }

        public Color ToolbarColour
            => _toolbarColour ??= ColorTranslator.FromHtml(Toolbar);

        public Brush ToolbarColourBrush
            => _toolbarColourBrush ??= new SolidBrush(ToolbarColour);

        public string Menu { get; set; }

        public Color MenuColour
            => _menuColour ??= ColorTranslator.FromHtml(Menu);

        public Brush MenuColourBrush
            => _menuColourBrush ??= new SolidBrush(MenuColour);

        public string MenuHighlight { get; set; }

        public Color MenuHighlightColour
            => _menuHighlightColour ??= ColorTranslator.FromHtml(MenuHighlight);

        public Brush MenuHighlightColourBrush
            => _menuHighlightColourBrush ??= new SolidBrush(MenuHighlightColour);

        public string IconSuffix
            => MenuColour.IsDark() ? "Dark" : null;

        public string Keyword { get; set; }

        public string Variable { get; set; }

        public string TypeName { get; set; }

        public string InterfaceName { get; set; }

        public string CommandStatement { get; set; }

        public string Text { get; set; }

        public string Numeric { get; set; }

        public string MethodName { get; set; }

        public string Comment { get; set; }

        public void ApplyTo(Control control)
        {
            if (control is ToolStrip toolStrip && !(toolStrip is ToolStripDropDown))
            {
                ApplyTo(toolStrip);
                return;
            }

            control.BackColor = MenuColour;
            control.ForeColor = ForeColour;

            if (control is IThemeable themeable)
            {
                themeable.Apply(this);
            }
        }

        public void ApplyTo(ToolStrip toolStrip)
        {
            toolStrip.BackColor = ToolbarColour;
            toolStrip.ForeColor = ForeColour;
        }
    }
}
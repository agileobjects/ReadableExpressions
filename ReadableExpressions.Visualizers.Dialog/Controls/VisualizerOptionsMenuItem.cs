namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Windows.Forms;

    internal class VisualizerOptionsMenuItem : ToolStripMenuItem, ILazyMenuItem
    {
        private readonly VisualizerDialog _dialog;
        private bool _initialized;

        public VisualizerOptionsMenuItem(VisualizerDialog dialog)
            : base("Options")
        {
            _dialog = dialog;
            ((ToolStripDropDownMenu)DropDown).ShowImageMargin = false;

            dialog.RegisterThemeable(DropDown);

            DropDownOpening += (sender, args) =>
                ((VisualizerOptionsMenuItem)sender).Populate();
        }

        public void Populate()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            DropDown.SuspendLayout();

            DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripControlHost(new ThemeSelector(_dialog)),
                new ToolStripControlHost(new FontSelector(_dialog)),
                new ToolStripControlHost(new ResizeToMatchCodeSamplesOption(_dialog)),
                new ToolStripSeparator(),
                new ToolStripControlHost(new FullyQualifiedTypeNamesOption(_dialog)),
                new ToolStripControlHost(new ExplicitTypeNamesOption(_dialog)),
                new ToolStripControlHost(new ExplicitGenericParamsOption(_dialog)),
                new ToolStripControlHost(new DeclareOutParamsInlineOption(_dialog)),
                new ToolStripControlHost(new ImplicitArrayTypeNamesOption(_dialog)),
                new ToolStripControlHost(new LambdaParameterTypeNamesOption(_dialog)),
                new ToolStripControlHost(new QuotedLambdaCommentsOption(_dialog))
            });

            DropDown.ResumeLayout();
        }

        void ILazyMenuItem.Initialize() => Populate();
    }
}
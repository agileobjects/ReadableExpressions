namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal interface ILazyMenu
    {
        void RegisterLazyMenuItem(ILazyMenuItem menuItem);
    }

    internal class VisualizerOptionsMenuItem : ToolStripMenuItem, ILazyMenu
    {
        private readonly List<ILazyMenuItem> _lazyMenuItems;

        public VisualizerOptionsMenuItem(VisualizerDialog dialog)
            : base("Options")
        {
            _lazyMenuItems = new List<ILazyMenuItem>();

            DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripControlHost(new ThemeSelector(dialog)),
                new ToolStripControlHost(new FontSelector(this, dialog)),
                new ToolStripControlHost(new FullyQualifiedTypeNamesOption(dialog)),
                new ToolStripControlHost(new ExplicitTypeNamesOption(dialog)),
                new ToolStripControlHost(new ExplicitGenericParamsOption(dialog)),
                new ToolStripControlHost(new DeclareOutParamsInlineOption(dialog)),
                new ToolStripControlHost(new ImplicitArrayTypeNamesOption(dialog)),
                new ToolStripControlHost(new LambdaParameterTypeNamesOption(dialog)),
                new ToolStripControlHost(new QuotedLambdaCommentsOption(dialog))
            });

            ((ToolStripDropDownMenu)DropDown).ShowImageMargin = false;

            dialog.RegisterThemeable(DropDown);

            DropDownOpening += (sender, args) =>
            {
                var menuItem = (VisualizerOptionsMenuItem)sender;

                foreach (var lazyMenuItem in menuItem._lazyMenuItems)
                {
                    lazyMenuItem.Initialize();
                }
            };
        }

        void ILazyMenu.RegisterLazyMenuItem(ILazyMenuItem menuItem) 
            => _lazyMenuItems.Add(menuItem);
    }
}
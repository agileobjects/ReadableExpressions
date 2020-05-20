namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using static Core.Theming.VisualizerDialogTheme;

    internal class ThemeSelector : MenuItemPanelBase
    {
        public ThemeSelector(VisualizerDialog dialog)
            : base(dialog)
        {
            var lightTheme = new ThemeOption(Light, dialog);
            var darkTheme = new ThemeOption(Dark, dialog);

            var label = new MenuItemLabel(
                "Theme",
                "Set the visualizer theme",
                lightTheme.Width + darkTheme.Width,
                dialog);

            Controls.Add(label);
            Controls.Add(lightTheme);
            Controls.Add(darkTheme);
        }
    }
}
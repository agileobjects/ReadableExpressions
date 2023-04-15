namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using Core.Theming;
    using static DialogConstants;

    internal class ThemeOption : RadioButton
    {
        private readonly VisualizerDialogTheme _theme;
        private readonly VisualizerDialog _dialog;

        public ThemeOption(
            VisualizerDialogTheme theme,
            VisualizerDialog dialog)
        {
            _theme = theme;
            _dialog = dialog;

            Size = new SizeF(
                ThemeOptionWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            Checked = theme.Name == dialog.Theme.Name;

            dialog.RegisterThemeable(this);

            base.Font = MenuItemFont;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = theme.Name;

            CheckedChanged += (sender, args) =>
            {
                var option = (ThemeOption)sender;

                if (option._dialog.Viewer.Uninitialised)
                {
                    return;
                }

                if (Checked && (option == this))
                {
                    option._dialog.HandleThemeChanged(option._theme);
                }
            };
        }
    }
}
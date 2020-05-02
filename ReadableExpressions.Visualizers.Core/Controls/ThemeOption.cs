namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using Theming;
    using static DialogConstants;

    internal class ThemeOption : RadioButton
    {
        private readonly ExpressionTranslationTheme _theme;
        private readonly VisualizerDialog _dialog;

        public ThemeOption(
            ExpressionTranslationTheme theme,
            VisualizerDialog dialog)
        {
            _theme = theme;
            _dialog = dialog;

            Size = new Size(ThemeOptionWidth, MenuItemHeight);
            Checked = theme.Name == dialog.Theme.Name;

            dialog.RegisterThemeable(this);

            base.Font = MenuItemFont;
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = theme.Name;

            CheckedChanged += (sender, args) =>
            {
                var option = (ThemeOption)sender;

                if (option._dialog.ViewerUninitialised)
                {
                    return;
                }

                if (Checked && (option == this))
                {
                    option._dialog.OnThemeChanged(option._theme);
                }
            };
        }
    }
}
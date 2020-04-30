namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Windows.Forms;
    using Theming;
    using static DialogConstants;

    internal class ThemeOption : RadioButton
    {
        public ThemeOption(
            ExpressionTranslationTheme theme,
            VisualizerDialog dialog)
        {
            var dialogTheme = dialog.Theme;

            Width = ThemeSelectorWidth / 2;
            Height = MenuItemHeight;
            Checked = theme == dialogTheme;

            dialog.RegisterThemeable(this);
            
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.Text = theme.Name;

            CheckedChanged += (sender, args) =>
            {
                if (dialog.ViewerUninitialised)
                {
                    return;
                }

                if ((sender == this) && Checked)
                {
                    dialog.OnThemeChanged(theme);
                }
            };
        }
    }
}
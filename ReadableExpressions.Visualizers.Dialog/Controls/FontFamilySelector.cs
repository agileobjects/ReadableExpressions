namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class FontFamilySelector : ComboBox
    {
        private static Font[] _fonts;

        private readonly VisualizerDialog _dialog;

        public FontFamilySelector(VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                FontSelectorWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DropDownStyle = ComboBoxStyle.DropDownList;

            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += WriteFontOption;

            DisplayMember = "Name";

            Items.AddRange(_fonts ??= GetFonts());

            var settingsName = _dialog.Settings.Font.Name;

            base.SelectedIndex = Array.FindIndex(_fonts, ff => ff.Name == settingsName);

            dialog.RegisterThemeable(this);

            SelectedIndexChanged += (sender, args) =>
            {
                var selector = (FontFamilySelector)sender;
                var font = (Font)selector.SelectedItem;

                selector._dialog.Viewer.SetFontFamily(font.Name);
                selector._dialog.Settings.Font.Name = font.Name;
                selector._dialog.Settings.Save();
            };
        }

        private static Font[] GetFonts()
        {
            return new InstalledFontCollection()
                .Families
                .Select(ff => new Font(ff, 10))
                .ToArray();
        }

        private static void WriteFontOption(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var selector = (FontFamilySelector)sender;
            var font = (Font)selector.Items[e.Index];
            var theme = selector._dialog.Theme;
            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            if (isSelected)
            {
                e.Graphics.FillRectangle(theme.MenuHighlightColourBrush, e.Bounds);
            }
            else
            {
                e.DrawBackground();
            }

            var fontBrush = isSelected || IsCommonCodingFont(font)
                ? theme.ForeColourBrush
                : theme.ForeLowlightColourBrush;

            e.Graphics.DrawString(font.Name, font, fontBrush, e.Bounds.X, e.Bounds.Y);
        }

        private static bool IsCommonCodingFont(Font font)
        {
            switch (font.Name)
            {
                case "Consolas":
                case "Courier":
                case "Courier New":
                case "Droid Sans":
                case "Fira Code":
                case "Input":
                case "Lucida Console":
                case "Lucida Sans Typewriter":
                case "Microsoft Sans Serif":
                case "Monoid":
                case "MS Gothic":
                case "MSimSun":
                case "OCR A Extended":
                case "Roboto":
                case "SimSun":
                case "SimSun-ExtB":
                case "Source Code Pro":
                case "Sudo":
                case "Terminal":
                case "Ubuntu Mono":
                    return true;
            }

            return false;
        }
    }
}
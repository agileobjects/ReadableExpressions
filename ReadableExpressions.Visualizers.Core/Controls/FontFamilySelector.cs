namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class FontFamilySelector : ComboBox, ILazyMenuItem
    {
        private static FontFamily[] _fontFamilies;

        private readonly VisualizerDialog _dialog;
        private bool _initialized;

        public FontFamilySelector(ILazyMenu menu, VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                FontSelectorWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DropDownStyle = ComboBoxStyle.DropDownList;

            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += WriteFontOption;

            DisplayMember = "Name";

            dialog.RegisterThemeable(this);
            menu.RegisterLazyItem(this);

            SelectedIndexChanged += (sender, args) =>
            {
                if (!_initialized)
                {
                    return;
                }

                var selector = (FontFamilySelector)sender;
                var fontFamily = (FontFamily)selector.SelectedItem;

                selector._dialog.Settings.Font.Name = fontFamily.Name;
                selector._dialog.Settings.Save();
                selector._dialog.UpdateTranslation();
            };
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            Items.AddRange(_fontFamilies ??= GetFontFamilies());

            var settingsName = _dialog.Settings.Font.Name;

            SelectedIndex = Array.FindIndex(_fontFamilies, ff => ff.Name == settingsName);

            _initialized = true;
        }

        private static FontFamily[] GetFontFamilies()
        {
            return new InstalledFontCollection()
                .Families
                .Where(IncludeFontFamily)
                .ToArray();
        }

        private static bool IncludeFontFamily(FontFamily fontFamily)
        {
            switch (fontFamily.Name)
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

        private static void WriteFontOption(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var selector = (FontFamilySelector)sender;
            var fontFamily = (FontFamily)selector.Items[e.Index];
            var font = new Font(fontFamily, selector.Font.SizeInPoints);
            var fontBrush = new SolidBrush(selector._dialog.Theme.ForeColour);

            e.DrawBackground();
            e.Graphics.DrawString(font.Name, font, fontBrush, e.Bounds.X, e.Bounds.Y);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal interface IInitializeableControl
    {
        void Initialize();
    }

    internal class FontFamilySelector : ComboBox, IInitializeableControl
    {
        private static readonly FontFamily[] _fontFamilies = new InstalledFontCollection()
            .Families
            .Where(f => f.IsStyleAvailable(FontStyle.Regular))
            .ToArray();

        public FontFamilySelector(VisualizerDialog dialog)
        {
            Size = new SizeF(
                FontSelectorWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DrawMode = DrawMode.Normal;
            DropDownStyle = ComboBoxStyle.DropDownList;

            DrawItem += WriteFontOption;

            dialog.RegisterThemeable(this);
            dialog.RegisterInitializable(this);
        }

        public void Initialize()
        {
            foreach (var fontFamily in _fontFamilies)
            {
                Items.Add(fontFamily.Name);
            }
        }

        private static void WriteFontOption(object sender, DrawItemEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var fontFamily = (FontFamily)comboBox.Items[e.Index];
            var font = new Font(fontFamily, comboBox.Font.SizeInPoints);

            e.DrawBackground();
            e.Graphics.DrawString(font.Name, font, Brushes.Black, e.Bounds.X, e.Bounds.Y);

            MessageBox.Show(font.Name + " written");
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal interface IInitializeableControl : IDisposable
    {
        void Initialize();
    }

    internal class FontFamilySelector : ComboBox, IInitializeableControl
    {
        private readonly VisualizerDialog _dialog;
        private readonly DrawItemEventHandler _drawItemHandler;

        public FontFamilySelector(VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                FontSelectorWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;

            _drawItemHandler = WriteFontOption;
            DrawItem += _drawItemHandler;

            DisplayMember = "Name";

            dialog.RegisterThemeable(this);
            dialog.RegisterInitializable(this);

            var fontFamilies = new InstalledFontCollection()
                .Families
                .Where(f => f.IsStyleAvailable(FontStyle.Regular))
                .ToArray<object>();

            Items.AddRange(fontFamilies);
        }

        public void Initialize()
        {
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

        protected override void Dispose(bool disposing)
        {
            Items.Clear();

            DrawItem -= _drawItemHandler;

            base.Dispose(disposing);
        }
    }
}
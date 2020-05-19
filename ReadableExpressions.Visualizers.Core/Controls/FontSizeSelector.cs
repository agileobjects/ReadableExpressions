namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class FontSizeSelector : ComboBox
    {
        private readonly VisualizerDialog _dialog;

        public FontSizeSelector(VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                40 * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DropDownStyle = ComboBoxStyle.DropDownList;

            Items.AddRange(Enumerable.Range(6, 18).Cast<object>().ToArray());

            SelectedItem = dialog.Settings.Font.Size;

            dialog.RegisterThemeable(this);

            SelectedIndexChanged += (sender, args) =>
            {
                var selector = (FontSizeSelector)sender;
                var fontSize = (int)selector.SelectedItem;

                selector._dialog.Viewer.SetFontSize(fontSize);
                selector._dialog.Settings.Font.Size = fontSize;
                selector._dialog.Settings.Save();
            };
        }
    }
}
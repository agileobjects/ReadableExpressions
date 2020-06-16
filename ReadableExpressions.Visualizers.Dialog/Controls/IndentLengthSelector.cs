namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using static Core.DialogConstants;

    internal class IndentLengthSelector : ComboBox
    {
        private readonly VisualizerDialog _dialog;

        public IndentLengthSelector(VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                40 * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DropDownStyle = ComboBoxStyle.DropDownList;

            Items.AddRange(Enumerable.Range(1, 8).Cast<object>().ToArray());

            SelectedItem = dialog.Settings.Indent.Length;

            dialog.RegisterThemeable(this);

            SelectedIndexChanged += (sender, args) =>
            {
                var selector = (IndentLengthSelector)sender;
                var settings = selector._dialog.Settings;
                var indentCharacter = settings.Indent.FirstOrDefault();
                var indentLength = (int)selector.SelectedItem;

                settings.Indent = new string(indentCharacter, indentLength);
                settings.Save();
                selector._dialog.UpdateTranslation();
            };
        }
    }
}
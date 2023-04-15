namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using static DialogConstants;

    internal class IndentTypeSelector : ComboBox
    {
        private static readonly KeyValuePair<string, char>[] _options = 
        {
            new KeyValuePair<string, char>("Spaces", ' '),
            new KeyValuePair<string, char>("Tabs", '\t')
        };

        private readonly VisualizerDialog _dialog;

        public IndentTypeSelector(VisualizerDialog dialog)
        {
            _dialog = dialog;

            Size = new SizeF(
                50 * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            DropDownStyle = ComboBoxStyle.DropDownList;

            DisplayMember = "Key";

            Items.AddRange(_options.Cast<object>().ToArray());

            base.SelectedIndex = dialog.Settings.Indent.FirstOrDefault() == ' ' ? 0 : 1;

            dialog.RegisterThemeable(this);

            SelectedIndexChanged += (sender, args) =>
            {
                var selector = (IndentTypeSelector)sender;
                var settings = selector._dialog.Settings;
                var indentCharacter = ((KeyValuePair<string, char>)selector.SelectedItem).Value;
                var indentLength = settings.Indent.Length;

                settings.Indent = new string(indentCharacter, indentLength);
                settings.Save();
                selector._dialog.UpdateTranslation();
            };
        }
    }
}
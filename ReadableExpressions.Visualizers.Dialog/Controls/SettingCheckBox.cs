namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using static Core.DialogConstants;

    internal class SettingCheckBox : CheckBox
    {
        private readonly Action<VisualizerDialog, bool> _optionSetter;
        private readonly VisualizerDialog _dialog;

        public SettingCheckBox(
            bool @checked,
            Action<VisualizerDialog, bool> optionSetter,
            VisualizerDialog dialog)
        {
            Checked = @checked;
            _optionSetter = optionSetter;
            _dialog = dialog;

            Size = new SizeF(
                SettingCheckBoxWidth * dialog.WidthFactor,
                MenuItemHeight * dialog.HeightFactor).ToSize();

            CheckAlign = ContentAlignment.MiddleRight;

            dialog.RegisterThemeable(this);

            CheckedChanged += (sender, args) =>
            {
                var checkbox = (SettingCheckBox)sender;

                checkbox._optionSetter.Invoke(checkbox._dialog, checkbox.Checked);
                checkbox._dialog.Settings.Save();
                checkbox._dialog.UpdateTranslation();
            };
        }
    }
}
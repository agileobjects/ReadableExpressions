namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using System.Windows.Forms;
    using static DialogConstants;

    internal abstract class VisualizerDialogOptionBase : MenuItemPanelBase
    {
        private readonly SettingCheckBox _checkBox;

        protected VisualizerDialogOptionBase(
            string labelText,
            string labelTooltip,
            bool isChecked,
            Action<VisualizerDialog, bool> optionSetter,
            VisualizerDialog dialog)
            : base(dialog)
        {
            var label = new MenuItemLabel(labelText, labelTooltip, SettingCheckBoxWidth, dialog);
            _checkBox = new SettingCheckBox(isChecked, optionSetter, dialog);

            label.Click += (sender, args) =>
            {
                var control = (Control)sender;
                var option = control as VisualizerDialogOptionBase;

                while (option == null)
                {
                    control = control.Parent;
                    option = control as VisualizerDialogOptionBase;
                }

                option._checkBox.Checked = !option._checkBox.Checked;
            };

            Controls.Add(label);
            Controls.Add(_checkBox);
        }
    }
}
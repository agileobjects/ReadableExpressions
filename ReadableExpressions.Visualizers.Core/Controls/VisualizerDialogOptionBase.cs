namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using static DialogConstants;

    internal abstract class VisualizerDialogOptionBase : MenuItemPanelBase
    {
        protected VisualizerDialogOptionBase(
            string labelText,
            string labelTooltip,
            bool isChecked,
            Action<VisualizerDialog, bool> optionSetter,
            VisualizerDialog dialog)
            : base(dialog)
        {
            var label = new MenuItemLabel(labelText, labelTooltip, SettingCheckBoxWidth, dialog);
            var checkbox = new SettingCheckBox(isChecked, optionSetter, dialog);

            label.Click += (sender, args) => checkbox.Checked = !checkbox.Checked;

            Controls.Add(label);
            Controls.Add(checkbox);
        }
    }
}
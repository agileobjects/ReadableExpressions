namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using static DialogConstants;

    internal abstract class VisualizerDialogOptionBase : MenuItemPanelBase
    {
        protected VisualizerDialogOptionBase(
            string labelText,
            bool isChecked,
            Action<VisualizerDialog, bool> optionSetter,
            VisualizerDialog dialog)
            : base(dialog)
        {
            var label = new MenuItemLabel(labelText, OptionControlWidth, dialog);
            var checkbox = new SettingCheckBox(isChecked, optionSetter, dialog);

            label.Click += (sender, args) => checkbox.Checked = !checkbox.Checked;

            Controls.Add(label);
            Controls.Add(checkbox);
        }
    }
}
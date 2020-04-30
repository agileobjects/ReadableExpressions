namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System;
    using System.Windows.Forms;
    using static DialogConstants;

    internal abstract class VisualizerDialogOptionBase : FlowLayoutPanel
    {
        protected VisualizerDialogOptionBase(
            string labelText,
            bool isChecked,
            Action<VisualizerDialog, bool> optionSetter,
            VisualizerDialog dialog)
        {
            FlowDirection = FlowDirection.LeftToRight;
            Width = MenuWidth;

            dialog.RegisterThemeable(this);

            var label = new MenuItemLabel(labelText, OptionControlWidth, dialog);
            var checkbox = new SettingCheckBox(isChecked, optionSetter, dialog);

            label.Click += (sender, args) => checkbox.Checked = !checkbox.Checked;

            Controls.Add(label);
            Controls.Add(checkbox);
        }
    }
}
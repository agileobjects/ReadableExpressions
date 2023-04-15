namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class FullyQualifiedTypeNamesOption : VisualizerDialogOptionBase
    {
        public FullyQualifiedTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Fully-qualify type names",
                "Fully qualify type names with their namespaces.",
                dialog.Settings.UseFullyQualifiedTypeNames,
                (dlg, isChecked) => dlg.Settings.UseFullyQualifiedTypeNames = isChecked,
                dialog)
        {
        }
    }
}
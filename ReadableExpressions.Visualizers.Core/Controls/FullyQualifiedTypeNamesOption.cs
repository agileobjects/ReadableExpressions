namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class FullyQualifiedTypeNamesOption : VisualizerDialogOptionBase
    {
        public FullyQualifiedTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Fully-qualify type names",
                dialog.Settings.UseFullyQualifiedTypeNames,
                (dlg, isChecked) => dlg.Settings.UseFullyQualifiedTypeNames = isChecked,
                dialog)
        {
        }
    }
}
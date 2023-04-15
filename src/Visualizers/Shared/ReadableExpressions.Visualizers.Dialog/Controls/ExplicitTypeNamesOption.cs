namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class ExplicitTypeNamesOption : VisualizerDialogOptionBase
    {
        public ExplicitTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Use explicit type names",
                "Use full type names instead of 'var' for local and " +
                "inline-declared output parameter variables.",
                dialog.Settings.UseExplicitTypeNames,
                (dlg, isChecked) => dlg.Settings.UseExplicitTypeNames = isChecked,
                dialog)
        {
        }
    }
}
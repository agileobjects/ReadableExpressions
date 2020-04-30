namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class ExplicitTypeNamesOption : VisualizerDialogOptionBase
    {
        public ExplicitTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Use explicit type names",
                dialog.Settings.UseExplicitTypeNames,
                (dlg, isChecked) => dlg.Settings.UseExplicitTypeNames = isChecked,
                dialog)
        {
        }
    }
}
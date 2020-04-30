namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class ImplicitArrayTypeNamesOption : VisualizerDialogOptionBase
    {
        public ImplicitArrayTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Show implicit array type names",
                dialog.Settings.ShowImplicitArrayTypes,
                (dlg, isChecked) => dlg.Settings.ShowImplicitArrayTypes = isChecked,
                dialog)
        {
        }
    }
}
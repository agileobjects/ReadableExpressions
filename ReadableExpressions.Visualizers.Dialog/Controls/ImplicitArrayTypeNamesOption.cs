namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class ImplicitArrayTypeNamesOption : VisualizerDialogOptionBase
    {
        public ImplicitArrayTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Show implicit array type names",
                "Show the names of implicitly-typed array types.",
                dialog.Settings.ShowImplicitArrayTypes,
                (dlg, isChecked) => dlg.Settings.ShowImplicitArrayTypes = isChecked,
                dialog)
        {
        }
    }
}
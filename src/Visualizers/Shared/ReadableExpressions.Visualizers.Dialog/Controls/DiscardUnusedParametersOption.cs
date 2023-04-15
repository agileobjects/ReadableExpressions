namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    internal class DiscardUnusedParametersOption : VisualizerDialogOptionBase
    {
        public DiscardUnusedParametersOption(VisualizerDialog dialog)
            : base(
                "Discard unused parameters",
                "Translate unused lambda or output parameter variables to discards (_).",
                dialog.Settings.DiscardUnusedParameters,
                (dlg, isChecked) => dlg.Settings.DiscardUnusedParameters = isChecked,
                dialog)
        {
        }
    }
}
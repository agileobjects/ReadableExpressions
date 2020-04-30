namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class DeclareOutParamsInlineOption : VisualizerDialogOptionBase
    {
        public DeclareOutParamsInlineOption(VisualizerDialog dialog)
            : base(
                "Declare out params inline",
                dialog.Settings.DeclareOutputParametersInline,
                (dlg, isChecked) => dlg.Settings.DeclareOutputParametersInline = isChecked,
                dialog)
        {
        }
    }
}
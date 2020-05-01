namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class DeclareOutParamsInlineOption : VisualizerDialogOptionBase
    {
        public DeclareOutParamsInlineOption(VisualizerDialog dialog)
            : base(
                "Declare out params inline",
                "Declare output parameter variables inline with the " +
                "method call where they are first used.",
                dialog.Settings.DeclareOutputParametersInline,
                (dlg, isChecked) => dlg.Settings.DeclareOutputParametersInline = isChecked,
                dialog)
        {
        }
    }
}
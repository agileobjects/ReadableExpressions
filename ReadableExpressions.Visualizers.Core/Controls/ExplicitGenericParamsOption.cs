namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class ExplicitGenericParamsOption : VisualizerDialogOptionBase
    {
        public ExplicitGenericParamsOption(VisualizerDialog dialog)
            : base(
                "Use explicit generic arguments",
                "Always specify generic parameter arguments explicitly in <pointy braces>",
                dialog.Settings.UseExplicitGenericParameters,
                (dlg, isChecked) => dlg.Settings.UseExplicitGenericParameters = isChecked,
                dialog)
        {
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class LambdaParameterTypeNamesOption : VisualizerDialogOptionBase
    {
        public LambdaParameterTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Show lambda parameter type names",
                dialog.Settings.ShowLambdaParameterTypeNames,
                (dlg, isChecked) => dlg.Settings.ShowLambdaParameterTypeNames = isChecked,
                dialog)
        {
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class LambdaParameterTypeNamesOption : VisualizerDialogOptionBase
    {
        public LambdaParameterTypeNamesOption(VisualizerDialog dialog)
            : base(
                "Show lambda parameter type names",
                "Show the names of lambda parameter types.",
                dialog.Settings.ShowLambdaParameterTypeNames,
                (dlg, isChecked) => dlg.Settings.ShowLambdaParameterTypeNames = isChecked,
                dialog)
        {
        }
    }
}
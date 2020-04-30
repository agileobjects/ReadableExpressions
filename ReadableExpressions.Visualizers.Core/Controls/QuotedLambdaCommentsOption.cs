namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class QuotedLambdaCommentsOption : VisualizerDialogOptionBase
    {
        public QuotedLambdaCommentsOption(VisualizerDialog dialog)
            : base(
                "Comment Quoted Lambdas",
                dialog.Settings.ShowQuotedLambdaComments,
                (dlg, isChecked) => dlg.Settings.ShowQuotedLambdaComments = isChecked,
                dialog)
        {
        }
    }
}
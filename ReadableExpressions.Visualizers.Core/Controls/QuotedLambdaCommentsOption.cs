namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    internal class QuotedLambdaCommentsOption : VisualizerDialogOptionBase
    {
        public QuotedLambdaCommentsOption(VisualizerDialog dialog)
            : base(
                "Comment Quoted Lambdas",
                "Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.",
                dialog.Settings.ShowQuotedLambdaComments,
                (dlg, isChecked) => dlg.Settings.ShowQuotedLambdaComments = isChecked,
                dialog)
        {
        }
    }
}
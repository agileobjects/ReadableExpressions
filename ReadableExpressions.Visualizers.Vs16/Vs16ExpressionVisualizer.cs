namespace AgileObjects.ReadableExpressions.Visualizers
{
    using Core;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs16ExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            windowService.ShowDialog(
                ExpressionDialog.Instance.WithText(
                    (string)objectProvider.GetObject()));
        }
    }
}

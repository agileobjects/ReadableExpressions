namespace AgileObjects.ReadableExpressions.Visualizers
{
    using Core;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs10ExpressionVisualizer : DialogDebuggerVisualizer
    {
        

        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            windowService.ShowDialog(
                ExpressionDialog.Instance.WithText(
                    objectProvider.GetObject().ToString()));
        }
    }
}

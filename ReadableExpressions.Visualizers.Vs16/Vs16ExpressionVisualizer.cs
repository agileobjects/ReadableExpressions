namespace AgileObjects.ReadableExpressions.Visualizers
{
    using Microsoft.VisualStudio.DebuggerVisualizers;
    using WpfVisualizerDialog = Dialog.Wpf.VisualizerDialog;

    public class Vs16ExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            new WpfVisualizerDialog(objectProvider.GetObject).ShowDialog();

            //using (var dialog = new VisualizerDialog(objectProvider.GetObject))
            //{
            //    windowService.ShowDialog(dialog);
            //}
        }
    }
}

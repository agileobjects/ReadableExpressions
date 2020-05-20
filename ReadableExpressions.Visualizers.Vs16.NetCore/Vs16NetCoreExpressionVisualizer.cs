namespace AgileObjects.ReadableExpressions.Visualizers.NetCore
{
    using System.Windows.Forms;
    using Dialog;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs16NetCoreExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            MessageBox.Show("This is .NET Core!");

            using (var dialog = new VisualizerDialog(objectProvider.GetObject))
            {
                windowService.ShowDialog(dialog);
            }
        }
    }
}
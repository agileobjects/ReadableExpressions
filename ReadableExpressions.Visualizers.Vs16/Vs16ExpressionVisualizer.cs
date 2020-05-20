﻿namespace AgileObjects.ReadableExpressions.Visualizers
{
    using System.Windows.Forms;
    using Core;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs16ExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            MessageBox.Show("This is .NET Framework!");

            using (var dialog = new VisualizerDialog(objectProvider.GetObject))
            {
                windowService.ShowDialog(dialog);
            }
        }
    }
}

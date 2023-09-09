﻿namespace AgileObjects.ReadableExpressions.Visualizers;

using Dialog;
using Microsoft.VisualStudio.DebuggerVisualizers;

public class Vs14ExpressionVisualizer : DialogDebuggerVisualizer
{
    protected override void Show(
        IDialogVisualizerService windowService,
        IVisualizerObjectProvider objectProvider)
    {
        windowService.ShowDialog(
            new VisualizerDialog(objectProvider.GetObject));
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers;

using System;
using Dialog;
using Microsoft.VisualStudio.DebuggerVisualizers;

public sealed class Vs18ExpressionVisualizer : DialogDebuggerVisualizer
{
    public Vs18ExpressionVisualizer() : base(FormatterPolicy.Json)
    {
    }

    protected override void Show(
        IDialogVisualizerService windowService,
        IVisualizerObjectProvider objectProvider)
    {
        Func<object> translationFactory =
            objectProvider is IVisualizerObjectProvider3 objectProvider3
            ? objectProvider3.GetObject<object>
#pragma warning disable CS0618 // Type or member is obsolete
            : objectProvider.GetObject;
#pragma warning restore CS0618            

        windowService.ShowDialog(
            new VisualizerDialog(translationFactory));
    }
}
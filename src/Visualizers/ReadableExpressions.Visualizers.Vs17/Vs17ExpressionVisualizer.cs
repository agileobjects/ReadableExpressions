namespace AgileObjects.ReadableExpressions.Visualizers;

using System;
using Dialog;
using Microsoft.VisualStudio.DebuggerVisualizers;

public class Vs17ExpressionVisualizer : DialogDebuggerVisualizer
{
    public Vs17ExpressionVisualizer() : base(FormatterPolicy.Json)
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
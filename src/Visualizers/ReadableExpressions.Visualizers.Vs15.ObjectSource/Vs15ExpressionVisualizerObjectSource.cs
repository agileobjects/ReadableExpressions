namespace AgileObjects.ReadableExpressions.Visualizers;

using Microsoft.VisualStudio.DebuggerVisualizers;
using ObjectSource;
using System.IO;

public sealed class Vs15ExpressionVisualizerObjectSource : VisualizerObjectSource
{
    public override void GetData(object target, Stream outgoingData) =>
        ExpressionVisualizerObjectSource.GetData(target, outgoingData, Serialize);
}
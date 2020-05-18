namespace AgileObjects.ReadableExpressions.Visualizers
{
    using System.IO;
    using Core;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs16ExpressionVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData) 
            => ExpressionVisualizerObjectSource.GetData(target, outgoingData, Serialize);
    }
}
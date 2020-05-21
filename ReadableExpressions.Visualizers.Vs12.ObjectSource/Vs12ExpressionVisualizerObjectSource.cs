namespace AgileObjects.ReadableExpressions.Visualizers
{
    using System.IO;
    using Microsoft.VisualStudio.DebuggerVisualizers;
    using ObjectSource;

    public class Vs12ExpressionVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData) 
            => ExpressionVisualizerObjectSource.GetData(target, outgoingData, Serialize);
    }
}
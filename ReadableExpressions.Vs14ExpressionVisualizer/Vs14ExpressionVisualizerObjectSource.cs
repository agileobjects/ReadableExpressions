namespace AgileObjects.ReadableExpressions.ExpressionVisualizer
{
    using System.IO;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs14ExpressionVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            ExpressionVisualizerObjectSource.GetData(target, outgoingData, Serialize);
        }
    }
}
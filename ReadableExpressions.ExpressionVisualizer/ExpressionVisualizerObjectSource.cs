namespace AgileObjects.ReadableExpressions.ExpressionVisualizer
{
    using System.IO;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class ExpressionVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var expression = (Expression)target;
            var readableExpression = expression.ToReadableString();

            Serialize(outgoingData, readableExpression);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.IO;
    using System.Linq.Expressions;

    public class ExpressionVisualizerObjectSource
    {
        public static void GetData(
            object target,
            Stream outgoingData,
            Action<Stream, string> serializer)
        {
            var expression = (Expression)target;
            var readableExpression = expression.ToReadableString();

            serializer.Invoke(outgoingData, readableExpression ?? "default(void)");
        }
    }
}
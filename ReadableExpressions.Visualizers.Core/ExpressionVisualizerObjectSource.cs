namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    public class ExpressionVisualizerObjectSource
    {
        public static void GetData(
            object target,
            Stream outgoingData,
            Action<Stream, string> serializer)
        {
            string value;

            switch (target)
            {
                case Expression expression:
                    value = expression.ToReadableString() ?? "default(void)";
                    break;

                case Type type:
                    value = type.GetFriendlyName();
                    break;

                case MethodInfo method:
                    value = "";
                    break;

                default:
                    return;
            }

            serializer.Invoke(outgoingData, value);
        }
    }
}
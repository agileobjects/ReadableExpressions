namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Translations.StaticTranslators;

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
                    value = MethodInfoTranslator.Translate(method);
                    break;

                default:
                    if (target == null)
                    {
                        return;
                    }

                    var targetType = target.GetType();

                    if (targetType.Name.StartsWith("System.Func", StringComparison.Ordinal) ||
                        targetType.Name.StartsWith("System.Action", StringComparison.Ordinal))
                    {
                        value = targetType.GetFriendlyName();
                        break;
                    }

                    return;
            }

            serializer.Invoke(outgoingData, value);
        }
    }
}
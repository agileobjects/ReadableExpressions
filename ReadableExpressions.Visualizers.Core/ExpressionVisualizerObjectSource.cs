namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Extensions;
    using Translations.StaticTranslators;

    public static class ExpressionVisualizerObjectSource
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
                    value = GetTranslationForVisualizer(expression) ?? "default(void)";
                    break;

                case Type type:
                    value = type.GetFriendlyName();
                    break;

                case MethodInfo method:
                    value = DefinitionsTranslator.Translate(method);
                    break;

                case ConstructorInfo ctor:
                    value = DefinitionsTranslator.Translate(ctor);
                    break;

                default:
                    if (target == null)
                    {
                        return;
                    }

                    value = target.GetType().GetFriendlyName();
                    break;
            }

            serializer.Invoke(outgoingData, value);
        }

        private static string GetTranslationForVisualizer(Expression expression)
        {
            return expression.ToReadableString(settings => ExpressionDialogSettings
                .Instance
                .Update(settings)
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }
    }
}
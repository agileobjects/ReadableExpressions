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
            var translated = GetTranslationFor(target, VisualizerDialogSettings.Instance);

            serializer.Invoke(outgoingData, translated);
        }

        internal static string GetTranslationFor(object target, VisualizerDialogSettings dialogSettings)
        {
            switch (target)
            {
                case Expression expression:
                    return GetTranslationForVisualizer(expression, dialogSettings) ?? "default(void)";

                case Type type:
                    return GetTranslationFor(type, dialogSettings);

                case MethodInfo method:
                    return DefinitionsTranslator.Translate(method);

                case ConstructorInfo ctor:
                    return DefinitionsTranslator.Translate(ctor);

                default:
                    if (target == null)
                    {
                        return string.Empty;
                    }

                    return GetTranslationFor(target.GetType(), dialogSettings);
            }
        }

        private static string GetTranslationFor(Type type, VisualizerDialogSettings dialogSettings)
            => type.GetFriendlyName(dialogSettings.Update);

        private static string GetTranslationForVisualizer(
            Expression expression,
            VisualizerDialogSettings dialogSettings)
        {
            return expression.ToReadableString(settings => dialogSettings
                .Update(settings)
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }
    }
}
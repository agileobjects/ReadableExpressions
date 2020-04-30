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
            var translated = GetTranslationFor(target);

            serializer.Invoke(outgoingData, translated);
        }

        internal static string GetTranslationFor(object target)
        {
            switch (target)
            {
                case Expression expression:
                    return Translate(expression) ?? "default(void)";

                case Type type:
                    return Translate(type);

                case MethodInfo method:
                    return DefinitionsTranslator.Translate(method);

                case ConstructorInfo ctor:
                    return DefinitionsTranslator.Translate(ctor);

                default:
                    if (target == null)
                    {
                        return string.Empty;
                    }

                    return Translate(target.GetType());
            }
        }

        private static string Translate(Type type)
            => type.GetFriendlyName(GetDialogSettings().Update);

        private static string Translate(Expression expression)
        {
            return expression.ToReadableString(settings => GetDialogSettings()
                .Update(settings)
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }

        private static VisualizerDialogSettings GetDialogSettings()
            => VisualizerDialogSettings.GetInstance();
    }
}
﻿namespace AgileObjects.ReadableExpressions.Visualizers.ObjectSource
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using Core.Configuration;
    using Extensions;
    using Formatting;
    using Translations.Formatting;

    public static class ExpressionVisualizerObjectSource
    {
        private static readonly ITranslationFormatter _htmlFormatter = TranslationHtmlFormatter.Instance;

        public static void GetData(
            object target,
            Stream outgoingData,
            Action<Stream, string> serializer)
        {
            var translated = GetTranslationFor(target) ?? "default(void)";

            serializer.Invoke(outgoingData, translated);
        }

        internal static string GetTranslationFor(object target)
        {
            switch (target)
            {
                case Expression expression:
                    return Translate(expression);

                case Type type:
                    return Translate(type);

                case MethodInfo method:
                    return method.ToReadableString(s => s.FormatUsing(_htmlFormatter));

                case ConstructorInfo ctor:
                    return ctor.ToReadableString(s => s.FormatUsing(_htmlFormatter));

                default:
                    if (target == null)
                    {
                        return string.Empty;
                    }

                    return Translate(target.GetType());
            }
        }

        private static string Translate(Type type)
        {
            return type.GetFriendlyName(settings => GetDialogSettings()
                .Update(settings)
                .FormatUsing(_htmlFormatter));
        }

        private static string Translate(Expression expression)
        {
            Debug.WriteLine("Translating: " + expression);

            return expression.ToReadableString(settings => GetDialogSettings()
                .Update(settings)
                .FormatUsing(_htmlFormatter));
        }

        private static VisualizerDialogSettings GetDialogSettings()
            => VisualizerDialogSettings.GetInstance();
    }
}
﻿namespace AgileObjects.ReadableExpressions.Visualizers.Core
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
    }
}
﻿namespace AgileObjects.ReadableExpressions.Translations.StaticTranslators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// Translates a MethodInfo object into a readable string. Used to provide MethodInfo visualization.
    /// </summary>
    public static class MethodDefinitionTranslator
    {
        /// <summary>
        /// Translates the given <paramref name="ctor"/> into a readable string.
        /// </summary>
        /// <param name="ctor">The MethodInfo to translate.</param>
        /// <returns>A readable string version of the given <paramref name="ctor"/>.</returns>
        public static string Translate(ConstructorInfo ctor)
        {
            if (ctor == null)
            {
                return "[Constructor not found]";
            }

            var buffer = new TranslationBuffer(ctor.ToString().Length);

            WriteModifiersToTranslation(ctor, buffer);

            buffer.WriteFriendlyName(ctor.DeclaringType);

            if (ctor.IsGenericMethod)
            {
                WriteGenericArgumentsToTranslation(ctor.GetGenericArguments(), buffer);
            }

            WriteParametersToTranslation(ctor, buffer);

            return buffer.GetContent();
        }

        /// <summary>
        /// Translates the given <paramref name="method"/> into a readable string.
        /// </summary>
        /// <param name="method">The MethodInfo to translate.</param>
        /// <returns>A readable string version of the given <paramref name="method"/>.</returns>
        public static string Translate(MethodInfo method)
        {
            if (method == null)
            {
                return "[Method not found]";
            }

            var buffer = new TranslationBuffer(method.ToString().Length);

            WriteModifiersToTranslation(method, buffer);

            if (method.IsStatic)
            {
                buffer.WriteToTranslation("static ");
            }

            var isProperty = method.IsPropertyGetterOrSetterCall(out var property);

            buffer.WriteFriendlyName(isProperty ? property.PropertyType : method.ReturnType);
            buffer.WriteSpaceToTranslation();

            if (method.DeclaringType != null)
            {
                buffer.WriteFriendlyName(method.DeclaringType);
                buffer.WriteToTranslation('.');
            }

            if (isProperty)
            {
                buffer.WriteToTranslation(property.Name);
                buffer.WriteToTranslation((method.ReturnType != typeof(void)) ? " { get; }" : " { set; }");

                return buffer.GetContent();
            }

            buffer.WriteToTranslation(method.Name);

            if (method.IsGenericMethod)
            {
                WriteGenericArgumentsToTranslation(method.GetGenericArguments(), buffer);
            }

            WriteParametersToTranslation(method, buffer);

            return buffer.GetContent();
        }

        private static void WriteModifiersToTranslation(ConstructorInfo ctor, TranslationBuffer buffer)
        {
            WriteAccessibilityToTranslation(ctor, buffer);

            if (ctor.DeclaringType.IsAbstract())
            {
                buffer.WriteToTranslation("abstract ");
            }
            else if (ctor.DeclaringType.IsSealed())
            {
                buffer.WriteToTranslation("sealed ");
            }
        }

        private static void WriteModifiersToTranslation(MethodInfo method, TranslationBuffer buffer)
        {
            WriteAccessibilityToTranslation(method, buffer);

            if (method.IsAbstract)
            {
                buffer.WriteToTranslation("abstract ");
            }
            else if (method.IsVirtual)
            {
                buffer.WriteToTranslation("virtual ");
            }
        }

        private static void WriteAccessibilityToTranslation(MethodBase method, TranslationBuffer buffer)
        {
            if (method.IsPublic)
            {
                buffer.WriteToTranslation("public ");
            }
            else if (method.IsAssembly)
            {
                buffer.WriteToTranslation("internal ");
            }
            else if (method.IsFamily)
            {
                buffer.WriteToTranslation("protected ");
            }
            else if (method.IsFamilyOrAssembly)
            {
                buffer.WriteToTranslation("protected internal ");
            }
            else if (method.IsPrivate)
            {
                buffer.WriteToTranslation("private ");
            }
        }

        private static void WriteGenericArgumentsToTranslation(IList<Type> genericArguments, TranslationBuffer buffer)
        {
            var genericArgumentTypes = genericArguments;

            buffer.WriteToTranslation('<');

            for (var i = 0; ;)
            {
                var argumentType = genericArgumentTypes[i];

                buffer.WriteFriendlyName(argumentType);

                if (++i == genericArgumentTypes.Count)
                {
                    break;
                }

                buffer.WriteToTranslation(", ");
            }

            buffer.WriteToTranslation('>');
        }

        private static void WriteParametersToTranslation(MethodBase method, TranslationBuffer buffer)
        {
            var parameters = method.GetParameters();

            if (!parameters.Any())
            {
                buffer.WriteToTranslation("()");
                return;
            }

            buffer.WriteNewLineToTranslation();
            buffer.WriteToTranslation('(');
            buffer.Indent();

            for (var i = 0; ;)
            {
                var parameter = parameters[i];
                var parameterType = parameter.ParameterType;

                buffer.WriteNewLineToTranslation();

                if (parameter.IsOut)
                {
                    buffer.WriteToTranslation("out ");
                    parameterType = parameterType.GetElementType();
                }
                else if (parameterType.IsByRef)
                {
                    buffer.WriteToTranslation("ref ");
                    parameterType = parameterType.GetElementType();
                }

                buffer.WriteFriendlyName(parameterType);
                buffer.WriteSpaceToTranslation();
                buffer.WriteToTranslation(parameter.Name);

                if (++i == parameters.Length)
                {
                    break;
                }

                buffer.WriteToTranslation(',');
            }

            buffer.Unindent();
            buffer.WriteNewLineToTranslation();
            buffer.WriteToTranslation(')');
        }
    }
}

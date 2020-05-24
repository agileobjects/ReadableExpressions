﻿namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using Translations;
    using static Translations.Formatting.TokenType;

    /// <summary>
    /// Provides reflection translation extension methods.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Translates this <paramref name="type"/> into a readable string.
        /// </summary>
        /// <param name="type">The Type to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="type"/>.</returns>
        public static string ToReadableString(
            this Type type, 
            Func<TranslationFormattingSettings, TranslationFormattingSettings> configuration = null)
        {
            if (type == null)
            {
                return "[Type not found]";
            }

            var settings = configuration.GetBufferSettings();
            var buffer = new TranslationBuffer(settings, type.ToString().Length);

            WriteModifiersToTranslation(type, buffer);

            buffer.WriteFriendlyName(type);

            return buffer.GetContent();
        }

        /// <summary>
        /// Translates this <paramref name="ctor"/> into a readable string.
        /// </summary>
        /// <param name="ctor">The ConstructorInfo to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="ctor"/>.</returns>
        public static string ToReadableString(
            this ConstructorInfo ctor, 
            Func<TranslationFormattingSettings, TranslationFormattingSettings> configuration = null)
        {
            if (ctor == null)
            {
                return "[Constructor not found]";
            }

            var settings = configuration.GetBufferSettings();
            var buffer = new TranslationBuffer(settings, ctor.ToString().Length);

            WriteAccessibilityToTranslation(ctor, buffer);

            buffer.WriteFriendlyName(ctor.DeclaringType);

            WriteParametersToTranslation(ctor, buffer);

            return buffer.GetContent();
        }

        /// <summary>
        /// Translates this <paramref name="method"/> into a readable string.
        /// </summary>
        /// <param name="method">The MethodInfo to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="method"/>.</returns>
        public static string ToReadableString(
            this MethodInfo method, 
            Func<TranslationFormattingSettings, TranslationFormattingSettings> configuration = null)
        {
            if (method == null)
            {
                return "[Method not found]";
            }
            
            var settings = configuration.GetBufferSettings();
            var buffer = new TranslationBuffer(settings, method.ToString().Length);

            WriteModifiersToTranslation(method, buffer);

            var isProperty = method.IsPropertyGetterOrSetterCall(out var property);

            buffer.WriteFriendlyName(isProperty ? property.PropertyType : method.ReturnType);
            buffer.WriteSpaceToTranslation();

            if (method.DeclaringType != null)
            {
                buffer.WriteFriendlyName(method.DeclaringType);
                buffer.WriteDotToTranslation();
            }

            if (isProperty)
            {
                buffer.WriteToTranslation(property.Name);
                buffer.WriteToTranslation(" { ");
                buffer.WriteKeywordToTranslation((method.ReturnType != typeof(void)) ? "get" : "set");
                buffer.WriteToTranslation("; }");

                return buffer.GetContent();
            }

            buffer.WriteToTranslation(method.Name, MethodName);

            if (method.IsGenericMethod)
            {
                WriteGenericArgumentsToTranslation(method.GetGenericArguments(), buffer);
            }

            WriteParametersToTranslation(method, buffer);

            return buffer.GetContent();
        }

        private static ITranslationBufferSettings GetBufferSettings(
            this Func<TranslationFormattingSettings, TranslationFormattingSettings> configuration)
        {
            return configuration?.Invoke(new TranslationFormattingSettings()) ?? TranslationFormattingSettings.Default;
        }

        private static void WriteModifiersToTranslation(Type type, TranslationBuffer buffer)
        {
            WriteAccessibilityToTranslation(type, buffer);

            if (type.IsInterface())
            {
                buffer.WriteKeywordToTranslation("interface ");
                return;
            }

            if (type.IsValueType())
            {
                buffer.WriteKeywordToTranslation("struct ");
                return;
            }
            
            if (type.IsAbstract())
            {
                buffer.WriteKeywordToTranslation(type.IsSealed() ? "static " : "abstract ");
            }
            else if (type.IsSealed())
            {
                buffer.WriteKeywordToTranslation("sealed ");
            }

            buffer.WriteKeywordToTranslation("class ");
        }

        private static void WriteAccessibilityToTranslation(Type type, TranslationBuffer buffer)
        {
            if (type.IsPublic())
            {
                buffer.WriteKeywordToTranslation("public ");
                return;
            }

            if (!type.IsNested)
            {
                buffer.WriteKeywordToTranslation("internal ");
                return;
            }
#if NETSTANDARD
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsNestedPublic)
#else
            if (type.IsNestedPublic)
#endif
            {
                buffer.WriteKeywordToTranslation("public ");
                return;
            }
#if NETSTANDARD
            if (typeInfo.IsNestedAssembly)
#else
            if (type.IsNestedAssembly)
#endif
            {
                buffer.WriteKeywordToTranslation("internal ");
                return;
            }
#if NETSTANDARD
            if (typeInfo.IsNestedFamORAssem)
#else
            if (type.IsNestedFamORAssem)
#endif
            {
                buffer.WriteKeywordToTranslation("protected internal ");
                return;
            }
#if NETSTANDARD
            if (typeInfo.IsNestedFamily)
#else
            if (type.IsNestedFamily)
#endif
            {
                buffer.WriteKeywordToTranslation("protected ");
                return;
            }
#if NETSTANDARD
            if (typeInfo.IsNestedPrivate)
#else
            if (type.IsNestedPrivate)
#endif
            {
                buffer.WriteKeywordToTranslation("private ");
            }
        }

        private static void WriteModifiersToTranslation(MethodBase method, TranslationBuffer buffer)
        {
            WriteAccessibilityToTranslation(method, buffer);

            if (method.IsAbstract)
            {
                buffer.WriteKeywordToTranslation("abstract ");
            }
            else
            {
                if (method.IsStatic)
                {
                    buffer.WriteKeywordToTranslation("static ");
                }

                if (method.IsVirtual)
                {
                    buffer.WriteKeywordToTranslation("virtual ");
                }
            }
        }

        private static void WriteAccessibilityToTranslation(MethodBase method, TranslationBuffer buffer)
        {
            if (method.IsPublic)
            {
                buffer.WriteKeywordToTranslation("public ");
            }
            else if (method.IsAssembly)
            {
                buffer.WriteKeywordToTranslation("internal ");
            }
            else if (method.IsFamily)
            {
                buffer.WriteKeywordToTranslation("protected ");
            }
            else if (method.IsFamilyOrAssembly)
            {
                buffer.WriteKeywordToTranslation("protected internal ");
            }
            else if (method.IsPrivate)
            {
                buffer.WriteKeywordToTranslation("private ");
            }
        }

        private static void WriteGenericArgumentsToTranslation(
            IList<Type> genericArguments, 
            TranslationBuffer buffer)
        {
            var genericArgumentTypes = genericArguments;

            buffer.WriteToTranslation('<');

            for (var i = 0; ;)
            {
                var argumentType = genericArgumentTypes[i];

                buffer.WriteFriendlyName(argumentType);

                ++i;

                if (i == genericArgumentTypes.Count)
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
                    buffer.WriteKeywordToTranslation("out ");
                    parameterType = parameterType.GetElementType();
                }
                else if (parameterType.IsByRef)
                {
                    buffer.WriteKeywordToTranslation("ref ");
                    parameterType = parameterType.GetElementType();
                }

                buffer.WriteFriendlyName(parameterType);
                buffer.WriteSpaceToTranslation();
                buffer.WriteToTranslation(parameter.Name, Variable);

                ++i;

                if (i == parameters.Length)
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

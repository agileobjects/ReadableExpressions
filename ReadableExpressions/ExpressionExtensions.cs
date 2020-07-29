﻿namespace AgileObjects.ReadableExpressions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using SourceCode.Api;
    using Translations;
#if NET35
    using LinqExpression = System.Linq.Expressions.Expression;
#endif

    /// <summary>
    /// Provides Expression translation extension methods.
    /// </summary>
    public static class ExpressionExtensions
    {
#if NET35
        /// <summary>
        /// Translates the given Linq <paramref name="expression"/> to a source-code string.
        /// </summary>
        /// <param name="expression">The Expression to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>.</returns>
        public static string ToReadableString(
            this LinqExpression expression,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            return LinqExpressionToDlrExpressionConverter
                .Convert(expression)
                .ToReadableString(configuration);
        }
#endif
        /// <summary>
        /// Translates the given <paramref name="expression"/> to a source-code string.
        /// </summary>
        /// <param name="expression">The Expression to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>.</returns>
        public static string ToReadableString(
            this Expression expression,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            if (expression == null)
            {
                return null;
            }

            var settings = configuration.GetTranslationSettings();
            var translation = new ExpressionTranslation(expression, settings);

            return translation.GetTranslation();
        }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to a complete source-code string,
        /// formatted as one or more classes with one or more methods in a namespace.
        /// </summary>
        /// <param name="expression">The Expression to translate to source code.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <remarks>
        /// To create multiple classes, supply a BlockExpression directly containing multiple
        /// LambdaExpressions. To generate a single class with multiple methods instead, use the
        /// CreateSingleClass configuration option.
        /// </remarks>
        /// <returns>
        /// The translated <paramref name="expression"/>, formatted as one or more classes with one
        /// or more methods in a namespace.
        /// </returns>
        public static string ToSourceCode(
            this Expression expression,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration = null)
        {
            return expression?
                .ToSourceCodeExpression(configuration)
                .ToSourceCode();
        }

        internal static TranslationSettings GetTranslationSettings(
            this Func<ITranslationSettings, ITranslationSettings> configuration)
        {
            if (configuration == null)
            {
                return TranslationSettings.Default;
            }

            var settings = new TranslationSettings();

            configuration.Invoke(settings);

            return settings;
        }
    }
}

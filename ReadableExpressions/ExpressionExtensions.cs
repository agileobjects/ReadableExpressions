﻿namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExpression = System.Linq.Expressions.Expression;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides the Expression translation extension method.
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
            Func<TranslationSettings, TranslationSettings> configuration = null)
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
            Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            if (expression == null)
            {
                return null;
            }

            var settings = GetTranslationSettings(configuration);
            var translation = new TranslationTree(expression, settings);

            return translation.GetTranslation();
        }

        internal static TranslationSettings GetTranslationSettings(
            Func<TranslationSettings, TranslationSettings> configuration)
        {
            return configuration?.Invoke(new TranslationSettings()) ?? TranslationSettings.Default;
        }
    }
}

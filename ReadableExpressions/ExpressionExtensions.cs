namespace AgileObjects.ReadableExpressions
{
    using System;
    using SourceCode;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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

            return expression.Translate(settings);
        }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to a complete source-code string,
        /// formatted as one or more classes with one or more methods in a namespace.
        /// </summary>
        /// <param name="expression">The Expression to translate to source code.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>
        /// The translated <paramref name="expression"/>, formatted as one or more classes with one
        /// or more methods in a namespace.
        /// </returns>
        public static string ToSourceCode(
            this Expression expression,
            Func<ISourceCodeTranslationSettings, ISourceCodeTranslationSettings> configuration = null)
        {
            if (expression == null)
            {
                return null;
            }

            return expression
                .ToSourceCodeExpression(configuration, out var settings)
                .Translate(settings);
        }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to a source-code string,
        /// formatted as a class.
        /// </summary>
        /// <param name="expression">The Expression to translate to source code.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>, formatted as a class.</returns>
        public static string ToSourceCodeClass(
            this Expression expression,
            Func<IClassTranslationSettings, IClassTranslationSettings> configuration = null)
        {
            if (expression == null)
            {
                return null;
            }

            return expression
                .ToClassExpression(configuration, out var settings)
                .Translate(settings);
        }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to a source-code string,
        /// formatted as a method.
        /// </summary>
        /// <param name="expression">The Expression to translate to source code.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>The translated <paramref name="expression"/>, formatted as a method.</returns>
        public static string ToSourceCodeMethod(
            this Expression expression,
            Func<IMethodTranslationSettings, IMethodTranslationSettings> configuration = null)
        {
            if (expression == null)
            {
                return null;
            }

            return expression
                .ToMethodExpression(configuration, out var settings)
                .Translate(settings);
        }

        private static string Translate(
            this Expression expression,
            TranslationSettings settings)
        {
            var translation = new ExpressionTranslation(expression, settings);

            return translation.GetTranslation();
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

namespace AgileObjects.ReadableExpressions
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

            var settings = configuration.GetTranslationSettings();
            var translation = new ExpressionTranslation(expression, settings);

            return translation.GetTranslation();
        }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to a complete source-code string,
        /// formatted as one or more classes with one or more methods in a namespace.
        /// </summary>
        /// <param name="expression">The Expression to translate to source code.</param>
        /// <returns>
        /// The translated <paramref name="expression"/>, formatted as one or more classes with one
        /// or more methods in a namespace.
        /// </returns>
        public static string ToSourceCode(this Expression expression) 
            => ReadableExpression.SourceCode(expression).ToReadableString();

        /// <summary>
        /// Translates the given <paramref name="lambda"/> to a source-code string,
        /// formatted as a method.
        /// </summary>
        /// <param name="lambda">The Expression to translate to source code.</param>
        /// <returns>The translated <paramref name="lambda"/>, formatted as a method.</returns>
        public static string ToSourceCodeMethod(this LambdaExpression lambda) 
            => ReadableExpression.Method(lambda).ToReadableString();

        internal static TranslationSettings GetTranslationSettings(
            this Func<TranslationSettings, TranslationSettings> configuration)
        {
            return configuration?.Invoke(new TranslationSettings()) ?? TranslationSettings.Default;
        }
    }
}

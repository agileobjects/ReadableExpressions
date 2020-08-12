namespace AgileObjects.ReadableExpressions.Build
{
    using System;
    using System.Linq.Expressions;
    using SourceCode.Api;

    /// <summary>
    /// Provides Source Code Expression translation extension methods.
    /// </summary>
    public static class SourceCodeExpressionExtensions
    {
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
    }
}

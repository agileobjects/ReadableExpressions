﻿namespace AgileObjects.ReadableExpressions
{
    using System;
    using Translations.Formatting;

    /// <summary>
    /// Provides configuration options to control aspects of source-code string generation.
    /// </summary>
    public interface ITranslationSettings
    {
        /// <summary>
        /// Fully qualify type names with their namespaces.
        /// </summary>
        ITranslationSettings UseFullyQualifiedTypeNames { get; }

        /// <summary>
        /// Use full type names instead of 'var' for local and inline-declared output parameter
        /// variables.
        /// </summary>
        ITranslationSettings UseExplicitTypeNames { get; }

        /// <summary>
        /// Always specify generic parameter arguments explicitly in &lt;pointy braces&gt;
        /// </summary>
        ITranslationSettings UseExplicitGenericParameters { get; }

        /// <summary>
        /// Declare output parameter variables inline with the method call where they are first used.
        /// </summary>
        ITranslationSettings DeclareOutputParametersInline { get; }

        /// <summary>
        /// Show the names of implicitly-typed array types.
        /// </summary>
        ITranslationSettings ShowImplicitArrayTypes { get; }

        /// <summary>
        /// Show the names of lambda parameter types.
        /// </summary>
        ITranslationSettings ShowLambdaParameterTypes { get; }

        /// <summary>
        /// Annotate Quoted Lambda Expressions with a comment indicating they have been Quoted.
        /// </summary>
        ITranslationSettings ShowQuotedLambdaComments { get; }

        /// <summary>
        /// Name anonymous types using the given <paramref name="nameFactory"/> instead of the
        /// default method.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory method to execute to retrieve the name for an anonymous type.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings NameAnonymousTypesUsing(Func<Type, string> nameFactory);

        /// <summary>
        /// Translate ConstantExpressions using the given <paramref name="valueFactory"/> instead of
        /// the default method.
        /// </summary>
        /// <param name="valueFactory">
        /// The factory method to execute to retrieve the ConstantExpression's translated value.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings TranslateConstantsUsing(Func<Type, object, string> valueFactory);

        /// <summary>
        /// Indent multi-line Expression translations using the given <paramref name="indent"/>.
        /// </summary>
        /// <param name="indent">
        /// The value with which to indent multi-line Expression translations.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings IndentUsing(string indent);

        /// <summary>
        /// Format Expression translations using the given <paramref name="formatter"/>.
        /// </summary>
        /// <param name="formatter">
        /// The <see cref="ITranslationFormatter"/> with which to format Expression translations.
        /// </param>
        /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
        ITranslationSettings FormatUsing(ITranslationFormatter formatter);
    }
}
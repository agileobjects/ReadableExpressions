namespace AgileObjects.ReadableExpressions;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Translations;
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
    /// Use discards (_) in place of unused parameters.
    /// </summary>
    ITranslationSettings DiscardUnusedParameters { get; }

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
    /// Show the value of any captured variables or members, instead of the captured variable
    /// or member name.
    /// </summary>
    ITranslationSettings ShowCapturedValues { get; }

    /// <summary>
    /// Show string.Concat() method calls with all-string arguments as method calls, instead of as
    /// concatenations using the concatenation operator (+).
    /// </summary>
    ITranslationSettings ShowStringConcatMethodCalls { get; }

    /// <summary>
    /// Use the given <paramref name="sourceCodeTranslationFactory"/> to translate Expressions with
    /// the given <paramref name="nodeType"/>.
    /// </summary>
    /// <param name="nodeType">
    /// The ExpressionType for which the given <paramref name="sourceCodeTranslationFactory"/> should
    /// be used.
    /// </param>
    /// <param name="sourceCodeTranslationFactory">
    /// A factory function which returns a source-code string translation for an Expression of the
    /// previously-specified ExpressionType. The factory is passed the Expression to translate and a
    /// factory function which can be used to translate child Expressions, or the original Expression
    /// if the default translation is desired.
    /// </param>
    /// <returns>These <see cref="ITranslationSettings"/>, to support a fluent API.</returns>
    ITranslationSettings AddTranslatorFor(
        ExpressionType nodeType,
        SourceCodeTranslationFactory sourceCodeTranslationFactory);

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
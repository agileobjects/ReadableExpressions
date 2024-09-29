namespace AgileObjects.ReadableExpressions.Translations;

using System;

/// <summary>
/// Defines a function to translate a given <paramref name="expression"/> to a source-code string.
/// </summary>
/// <param name="expression">The Expression to translate.</param>
/// <param name="defaultTranslationFactory">
/// A factory which returns a source-code string for a given Expression. Can be used to translate
/// child Expressions of the given <paramref name="expression"/>, or to translate the given
/// <paramref name="expression"/> if a custom translation is not desired.
/// </param>
/// <returns>A source-code string representation of the given <paramref name="expression"/>.</returns>
public delegate string SourceCodeTranslationFactory(
    Expression expression,
    Func<Expression, string> defaultTranslationFactory);
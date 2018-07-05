namespace AgileObjects.ReadableExpressions.Translators
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Defines a method which translates an <see cref="Expression"/> into a source code string.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to translate.</param>
    /// <param name="context">
    /// The <see cref="TranslationContext"/> for the root <see cref="Expression"/> being translated.
    /// </param>
    /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
    public delegate string Translator(Expression expression, TranslationContext context);
}
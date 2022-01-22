namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Implementing classes will provide information about an Expression translation's context.
    /// </summary>
    public interface ITranslationContext
    {
        /// <summary>
        /// Gets the <see cref="TranslationSettings"/> to use for translation in this context.
        /// </summary>
        TranslationSettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="ExpressionAnalysis"/> containing information about the root
        /// Expression being translated.
        /// </summary>
        ExpressionAnalysis Analysis { get; }

        /// <summary>
        /// Gets the <see cref="ITranslation"/> for the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The Expression for which to get an <see cref="ITranslation"/>.</param>
        /// <returns>The <see cref="ITranslation"/> for the given <paramref name="expression"/>.</returns>
        ITranslation GetTranslationFor(Expression expression);
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// Implementing custom Expression classes will provide their own <see cref="ITranslation"/> for
    /// use during an Expression translation.
    /// </summary>
    public interface ICustomTranslationExpression
    {
        /// <summary>
        /// Gets the <see cref="ITranslation"/> to use to translate this
        /// <see cref="ICustomTranslationExpression"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the Expression translation taking place.
        /// </param>
        /// <returns>
        /// The <see cref="ITranslation"/> to use to translate this <see cref="ICustomTranslationExpression"/>.
        /// </returns>
        ITranslation GetTranslation(ITranslationContext context);
    }
}

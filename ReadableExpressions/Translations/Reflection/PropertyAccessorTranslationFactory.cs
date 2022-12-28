namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Creates an <see cref="ITranslation"/> for the given <paramref name="parent"/>'s
    /// <paramref name="accessor"/>.
    /// </summary>
    /// <param name="parent">
    /// The <see cref="PropertyDefinitionTranslation"/> to which the <paramref name="accessor"/>
    /// belongs.
    /// </param>
    /// <param name="accessor">
    /// An <see cref="IComplexMember"/> representing the accessor for which to create the
    /// <see cref="ITranslation"/>.
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    /// <returns>An <see cref="ITranslation"/> which translates the given <paramref name="accessor"/>.</returns>
    public delegate ITranslation PropertyAccessorTranslationFactory(
        PropertyDefinitionTranslation parent,
        IComplexMember accessor,
        TranslationSettings settings);
}
namespace AgileObjects.ReadableExpressions.Extensions;

using System;
using Translations;
using Translations.Reflection;

/// <summary>
/// Provides extension methods to use with a <see cref="ITranslationContext"/>.
/// </summary>
public static class PublicTranslationContextExtensions
{
    /// <summary>
    /// Gets a <see cref="CodeBlockTranslation"/> for the given <paramref name="expression"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> with which to create the <see cref="INodeTranslation"/>.
    /// </param>
    /// <param name="expression">The Expression for which to get a <see cref="CodeBlockTranslation"/>.</param>
    /// <returns>A <see cref="CodeBlockTranslation"/> for the given <paramref name="expression"/>.</returns>
    public static CodeBlockTranslation GetCodeBlockTranslationFor(
        this ITranslationContext context,
        Expression expression)
    {
        return new(context.GetTranslationFor(expression), context);
    }

    /// <summary>
    /// Gets the <see cref="INodeTranslation"/> for the given <paramref name="type"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> with which to create the <see cref="INodeTranslation"/>.
    /// </param>
    /// <param name="type">The Type for which to get an <see cref="INodeTranslation"/>.</param>
    /// <returns>The <see cref="INodeTranslation"/> for the given <paramref name="type"/>.</returns>
    public static TypeNameTranslation GetTranslationFor(
        this ITranslationContext context,
        Type type)
    {
        return new(type, context.Settings);
    }

    /// <summary>
    /// Gets the <see cref="INodeTranslation"/> for the given <paramref name="type"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> with which to create the <see cref="INodeTranslation"/>.
    /// </param>
    /// <param name="type">The <see cref="IType"/> for which to get an <see cref="INodeTranslation"/>.</param>
    /// <returns>The <see cref="INodeTranslation"/> for the given <paramref name="type"/>.</returns>
    public static TypeNameTranslation GetTranslationFor(
        this ITranslationContext context,
        IType type)
    {
        return new(type, context.Settings);
    }
}
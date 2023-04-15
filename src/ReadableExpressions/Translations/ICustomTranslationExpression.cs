namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

/// <summary>
/// Implementing custom Expression classes will provide their own <see cref="INodeTranslation"/> for
/// use during an Expression translation.
/// </summary>
public interface ICustomTranslationExpression
{
    /// <summary>
    /// Gets the <see cref="INodeTranslation"/> to use to translate this
    /// <see cref="ICustomTranslationExpression"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> describing the Expression translation taking place.
    /// </param>
    /// <returns>
    /// The <see cref="INodeTranslation"/> to use to translate this <see cref="ICustomTranslationExpression"/>.
    /// </returns>
    INodeTranslation GetTranslation(ITranslationContext context);
}
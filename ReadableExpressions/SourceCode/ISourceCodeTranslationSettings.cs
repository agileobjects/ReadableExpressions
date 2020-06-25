namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides configuration options to control aspects of source-code generation.
    /// </summary>
    public interface ISourceCodeTranslationSettings : ITranslationSettings
    {
        /// <summary>
        /// Add generated classes to the given <paramref name="namespace"/>.
        /// </summary>
        /// <param name="namespace">The namespace in which to create generated classes.</param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings WithNamespace(string @namespace);

        /// <summary>
        /// Name generated classes using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated class. The suppled Expression
        /// will be the nearest-scoped Expression.
        /// </param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings NameClassesUsing(Func<Expression, string> nameFactory);
    }
}
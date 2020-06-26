namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;

    /// <summary>
    /// Provides configuration options to control aspects of source-code generation.
    /// </summary>
    public interface ISourceCodeTranslationSettings : 
        IClassTranslationSettings<ISourceCodeTranslationSettings>
    {
        /// <summary>
        /// Add generated classes to the namespace of the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type the namespace of which the generated code should use.</typeparam>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings WithNamespaceOf<T>();

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
        /// The factory from which to obtain the name of a generated class. The parent
        /// <see cref="SourceCodeExpression"/> and <see cref="ClassExpression"/> are supplied.
        /// </param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings NameClassesUsing(
            Func<SourceCodeExpression, ClassExpression, string> nameFactory);
    }
}
namespace AgileObjects.ReadableExpressions.Build.SourceCode.Api
{
    using System;

    /// <summary>
    /// Provides configuration options to control aspects of <see cref="SourceCodeExpression"/>
    /// creation.
    /// </summary>
    public interface ISourceCodeExpressionSettings : ITranslationSettings
    {
        /// <summary>
        /// Add generated classes to the namespace of the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type the namespace of which the generated code should use.</typeparam>
        /// <returns>These <see cref="ISourceCodeExpressionSettings"/>, to support a fluent interface.</returns>
        ISourceCodeExpressionSettings WithNamespaceOf<T>();

        /// <summary>
        /// Add generated classes to the given <paramref name="namespace"/>.
        /// </summary>
        /// <param name="namespace">The namespace in which to create generated classes.</param>
        /// <returns>These <see cref="ISourceCodeExpressionSettings"/>, to support a fluent interface.</returns>
        ISourceCodeExpressionSettings WithNamespace(string @namespace);

        /// <summary>
        /// Add a <see cref="ClassExpression"/> to the <see cref="SourceCodeExpression"/> being built,
        /// using the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">
        /// The configuration with which to generate the <see cref="ClassExpression"/>.
        /// </param>
        /// <returns>These <see cref="ISourceCodeExpressionSettings"/>, to support a fluent interface.</returns>
        ISourceCodeExpressionSettings WithClass(
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration);

        /// <summary>
        /// Add a <see cref="ClassExpression"/> to the <see cref="SourceCodeExpression"/> being built,
        /// using the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="ClassExpression"/> to create.</param>
        /// <param name="configuration">
        /// The configuration with which to generate the <see cref="ClassExpression"/>.
        /// </param>
        /// <returns>These <see cref="ISourceCodeExpressionSettings"/>, to support a fluent interface.</returns>
        ISourceCodeExpressionSettings WithClass(
            string name,
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration);

        /// <summary>
        /// Add a <see cref="ClassExpression"/> to the <see cref="SourceCodeExpression"/> being built,
        /// using the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="ClassExpression"/> to create.</param>
        /// <param name="summary">The summary documentation of the <see cref="ClassExpression"/> to create.</param>
        /// <param name="configuration">
        /// The configuration with which to generate the <see cref="ClassExpression"/>.
        /// </param>
        /// <returns>These <see cref="ISourceCodeExpressionSettings"/>, to support a fluent interface.</returns>
        ISourceCodeExpressionSettings WithClass(
            string name,
            string summary,
            Func<IClassExpressionSettings, IClassExpressionSettings> configuration);
    }
}
namespace AgileObjects.ReadableExpressions.SourceCode.Api
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides configuration options to control aspects of <see cref="ClassExpression"/> creation.
    /// </summary>
    public interface IClassExpressionSettings
    {
        /// <summary>
        /// Configures the <see cref="ClassExpression"/> being built to implement the given
        /// <typeparamref name="TInterface"/> .
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of interface the <see cref="ClassExpression"/> being built should implement.
        /// </typeparam>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings Implementing<TInterface>() where TInterface : class;

        /// <summary>
        /// Configures the <see cref="ClassExpression"/> being built to implement the given
        /// <paramref name="interfaces"/> .
        /// </summary>
        /// <param name="interfaces">
        /// The type of interfaces the <see cref="ClassExpression"/> being built should implement.
        /// </param>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings Implementing(params Type[] interfaces);

        /// <summary>
        /// Add a <see cref="MethodExpression"/> to the <see cref="ClassExpression"/> being built,
        /// using the given <paramref name="body"/>.
        /// </summary>
        /// <param name="body">
        /// The Expression from which to create the <see cref="MethodExpression"/>'s parameters and
        /// body.
        /// </param>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings WithMethod(Expression body);

        /// <summary>
        /// Add a <see cref="MethodExpression"/> to the <see cref="ClassExpression"/> being built,
        /// using the given <paramref name="name"/> and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="MethodExpression"/> to create.</param>
        /// <param name="body">
        /// The Expression from which to create the <see cref="MethodExpression"/>'s parameters and
        /// body.
        /// </param>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings WithMethod(string name, Expression body);

        /// <summary>
        /// Add a <see cref="MethodExpression"/> to the <see cref="ClassExpression"/> being built,
        /// using the given <paramref name="name"/> and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="MethodExpression"/> to create.</param>
        /// <param name="summary">The summary documentation of the <see cref="MethodExpression"/> to create.</param>
        /// <param name="body">
        /// The Expression from which to create the <see cref="MethodExpression"/>'s parameters and
        /// body.
        /// </param>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings WithMethod(string name, string summary, Expression body);
    }
}
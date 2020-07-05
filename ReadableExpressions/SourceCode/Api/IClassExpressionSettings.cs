namespace AgileObjects.ReadableExpressions.SourceCode.Api
{
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
        /// Add a <see cref="MethodExpression"/> to the <see cref="ClassExpression"/> being built,
        /// using the given <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">
        /// The LambdaExpression from which to create the <see cref="MethodExpression"/>'s parameters
        /// and body.
        /// </param>
        /// <returns>These <see cref="IClassExpressionSettings"/>, to support a fluent interface.</returns>
        IClassExpressionSettings WithMethod(LambdaExpression definition);
    }
}
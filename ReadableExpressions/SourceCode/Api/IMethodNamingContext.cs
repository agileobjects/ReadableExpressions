namespace AgileObjects.ReadableExpressions.SourceCode.Api
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides information with which to name a <see cref="MethodExpression"/>.
    /// </summary>
    public interface IMethodNamingContext
    {
        /// <summary>
        /// Gets the return type of the LambdaExpression from which the method to which this
        /// <see cref="IMethodNamingContext"/> relates was created.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Gets the LambdaExpression from which the method to which this
        /// <see cref="IMethodNamingContext"/> relates was created.
        /// </summary>
        LambdaExpression Body { get; }

        /// <summary>
        /// Gets the index of the method in the set of generated class methods to which this
        /// <see cref="IMethodNamingContext"/> relates.
        /// </summary>
        int Index { get; }
    }
}
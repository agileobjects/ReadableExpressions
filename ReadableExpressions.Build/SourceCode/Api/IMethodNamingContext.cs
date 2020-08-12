﻿namespace AgileObjects.ReadableExpressions.Build.SourceCode.Api
{
    using System;
    using System.Linq.Expressions;

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
        /// Gets a PascalCase, method-name-friendly translation of the return type of the
        /// LambdaExpression from which the method to which this <see cref="IMethodNamingContext"/>
        /// relates was created.
        /// </summary>
        string ReturnTypeName { get; }

        /// <summary>
        /// Gets the LambdaExpression from which the method to which this
        /// <see cref="IMethodNamingContext"/> relates was created.
        /// </summary>
        LambdaExpression MethodLambda { get; }

        /// <summary>
        /// Gets the index of the method in the set of generated class methods to which this
        /// <see cref="IMethodNamingContext"/> relates.
        /// </summary>
        int Index { get; }
    }
}
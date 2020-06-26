namespace AgileObjects.ReadableExpressions.SourceCode.Api
{
    using System;
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides information with which to name a <see cref="ClassExpression"/>.
    /// </summary>
    public interface IClassNamingContext
    {
        /// <summary>
        /// Gets the ExpressionType of the Expression from which this <see cref="IClassNamingContext"/>
        /// was created.
        /// </summary>
        ExpressionType NodeType { get; }

        /// <summary>
        /// Gets the return type of the Expression from which the main method of the class to which
        /// this <see cref="IClassNamingContext"/> relates was created.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the Expression from which this <see cref="IClassNamingContext"/> was created.
        /// </summary>
        Expression Body { get; }

        /// <summary>
        /// Gets the <see cref="MethodExpression"/>s which make up this
        /// <see cref="IClassNamingContext"/>'s methods.
        /// </summary>
        ReadOnlyCollection<MethodExpression> Methods { get; }

        /// <summary>
        /// Gets the index of the class in the set of generated classes to which this
        /// <see cref="IClassNamingContext"/> relates.
        /// </summary>
        int Index { get; }
    }
}
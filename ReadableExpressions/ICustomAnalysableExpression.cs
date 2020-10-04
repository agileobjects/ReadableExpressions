namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Implementing custom Expression classes will provide BCL Expressions for analysis.
    /// </summary>
    public interface ICustomAnalysableExpression
    {
        /// <summary>
        /// Gets the BCL Expressions contained by this <see cref="ICustomAnalysableExpression"/>.
        /// </summary>
        IEnumerable<Expression> Expressions { get; }
    }
}
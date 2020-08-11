﻿namespace AgileObjects.ReadableExpressions
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides constant values used by ReadableExpressions. This type is not intended to be used
    /// by your code.
    /// </summary>
    public static class ReadableExpressionConstants
    {
        /// <summary>
        /// Gets the numeric value used for a <see cref="CommentExpression"/>'s ExpressionType.
        /// </summary>
        public const ExpressionType ExpressionTypeComment = (ExpressionType)1004;
    }
}
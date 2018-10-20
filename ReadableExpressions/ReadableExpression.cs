﻿namespace AgileObjects.ReadableExpressions
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    /// <summary>
    /// A factory class providing the Comment Expression
    /// </summary>
    public static class ReadableExpression
    {
        /// <summary>
        /// Create a <see cref="ConstantExpression"/> representing a code comment with the 
        /// given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text of the comment to create, without slashes or /* */.</param>
        /// <returns>A <see cref="ConstantExpression"/> representing a code comment.</returns>
        public static ConstantExpression Comment(string text)
            => Expression.Constant(text.AsComment());
    }
}

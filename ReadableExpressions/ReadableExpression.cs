namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

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
        {
            return Expression.Constant(text.AsComment());
        }
    }
}

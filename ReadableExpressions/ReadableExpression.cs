namespace AgileObjects.ReadableExpressions
{
    using SourceCode;

    /// <summary>
    /// A factory class providing the Comment Expression.
    /// </summary>
    public static class ReadableExpression
    {
        /// <summary>
        /// Create a <see cref="CommentExpression"/> representing a code comment with the 
        /// given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text of the comment to create, without slashes or /* */.</param>
        /// <returns>A <see cref="CommentExpression"/> representing a code comment.</returns>
        public static CommentExpression Comment(string text)
            => new CommentExpression(text);
    }
}

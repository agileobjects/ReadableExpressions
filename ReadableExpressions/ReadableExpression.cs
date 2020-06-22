namespace AgileObjects.ReadableExpressions
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using SourceCode;

    /// <summary>
    /// A factory class providing source code Expressions.
    /// </summary>
    public static class ReadableExpression
    {
        /// <summary>
        /// Create a <see cref="SourceCodeExpression"/> representing a complete piece of source code
        /// with the given <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The content of the piece of source code to create.</param>
        /// <returns>A <see cref="SourceCodeExpression"/> representing a complete piece of source code.</returns>
        public static SourceCodeExpression SourceCode(Expression content)
            => new SourceCodeExpression(content);

        /// <summary>
        /// Create a <see cref="ClassExpression"/> representing a source code class with the given
        /// <paramref name="singleMethodContent"/>.
        /// </summary>
        /// <param name="singleMethodContent">
        /// A LambdaExpression defining the <see cref="ClassExpression"/>'s single method.
        /// </param>
        /// <returns>A <see cref="ClassExpression"/> representing a source code class.</returns>
        public static ClassExpression Class(LambdaExpression singleMethodContent)
            => new ClassExpression(singleMethodContent);

        /// <summary>
        /// Create a <see cref="MethodExpression"/> representing a source code method with the given
        /// <paramref name="method"/> definition.
        /// </summary>
        /// <param name="method">A LambdaExpression defining the <see cref="MethodExpression"/>.</param>
        /// <returns>A <see cref="MethodExpression"/> representing a source code method.</returns>
        public static MethodExpression Method(LambdaExpression method)
            => new MethodExpression(method);

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

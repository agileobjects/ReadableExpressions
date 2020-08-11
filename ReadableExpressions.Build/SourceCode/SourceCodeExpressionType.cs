namespace AgileObjects.ReadableExpressions.Build.SourceCode
{
    using static ReadableExpressionConstants;

    /// <summary>
    /// Defines ExpressionType value options for source code Expressions.
    /// </summary>
    public enum SourceCodeExpressionType
    {
        /// <summary>
        /// 1000. A piece of source code.
        /// </summary>
        SourceCode = 1000,

        /// <summary>
        /// 1001. A source code class.
        /// </summary>
        Class = 1001,

        /// <summary>
        /// 1002. A source code class method.
        /// </summary>
        Method = 1002,

        /// <summary>
        /// 1003. A source code method parameter.
        /// </summary>
        MethodParameter = 1003,

        /// <summary>
        /// 1004. A source code comment.
        /// </summary>
        Comment = ExpressionTypeComment
    }
}

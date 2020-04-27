namespace AgileObjects.ReadableExpressions.Translations
{
    /// <summary>
    /// Defines types of source code elements.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// A default source code element.
        /// </summary>
        Default,

        /// <summary>
        /// A language keyword (new, int, default, etc).
        /// </summary>
        Keyword,

        /// <summary>
        /// A local variable or method or constructor parameter.
        /// </summary>
        Variable,

        /// <summary>
        /// The name of a type.
        /// </summary>
        TypeName,

        /// <summary>
        /// The name of an interface.
        /// </summary>
        InterfaceName,

        /// <summary>
        /// A control statement (if, else, return, etc).
        /// </summary>
        ControlStatement,

        /// <summary>
        /// A constant string or character value.
        /// </summary>
        Text,

        /// <summary>
        /// A constant numeric value.
        /// </summary>
        Numeric,

        /// <summary>
        /// The name of a method.
        /// </summary>
        MethodName,

        /// <summary>
        /// A code comment.
        /// </summary>
        Comment
    }
}
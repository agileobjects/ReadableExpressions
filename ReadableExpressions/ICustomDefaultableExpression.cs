namespace AgileObjects.ReadableExpressions
{
    /// <summary>
    /// Implementing custom Expression classes will determine if their default value can be translated
    /// with the null keyword.
    /// </summary>
    public interface ICustomDefaultableExpression
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="ICustomDefaultableExpression"/>'s default
        /// value can be represented with a null keyword.
        /// </summary>
        bool AllowNullKeyword { get; }
    }
}
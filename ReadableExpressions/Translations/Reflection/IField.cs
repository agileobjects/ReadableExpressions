namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a type field.
    /// </summary>
    public interface IField : IMember
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IField"/> is readonly.
        /// </summary>
        bool IsReadonly { get; }
    }
}
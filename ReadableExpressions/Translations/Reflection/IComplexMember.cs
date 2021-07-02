namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a property or method.
    /// </summary>
    public interface IComplexMember : IMember
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IComplexMember"/> is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IComplexMember"/> is virtual.
        /// </summary>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IComplexMember"/> is an override.
        /// </summary>
        bool IsOverride { get; }
    }
}
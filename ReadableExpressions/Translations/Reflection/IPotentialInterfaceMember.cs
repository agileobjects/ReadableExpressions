namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a member which potentially belongs to an
    /// interface.
    /// </summary>
    public interface IPotentialInterfaceMember : IMember
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IPotentialInterfaceMember"/> belongs to
        /// an interface.
        /// </summary>
        bool IsInterfaceMember { get; }
    }
}
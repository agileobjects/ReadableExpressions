namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a type member.
    /// </summary>
    public interface IMember
    {
        /// <summary>
        /// Gets the <see cref="IType"/> which declares the <see cref="IMember"/>.
        /// </summary>
        IType DeclaringType { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is static.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is public.
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is internal.
        /// </summary>
        bool IsInternal { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is protected internal.
        /// </summary>
        bool IsProtectedInternal { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is protected.
        /// </summary>
        bool IsProtected { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is private protected.
        /// </summary>
        bool IsPrivateProtected { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMember"/> is private.
        /// </summary>
        bool IsPrivate { get; }

        /// <summary>
        /// Gets the name of the <see cref="IMember"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="IType"/> of the <see cref="IMember"/>.
        /// </summary>
        IType Type { get; }
    }
}
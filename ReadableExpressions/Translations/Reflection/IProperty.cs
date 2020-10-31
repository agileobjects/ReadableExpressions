namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a property.
    /// </summary>
    public interface IProperty : IComplexMember
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IProperty"/> has a public
        /// <see cref="Getter"/>.
        /// </summary>
        bool IsReadable { get; }

        /// <summary>
        /// Gets an <see cref="IMember"/> describing the <see cref="IProperty"/>'s getter.
        /// </summary>
        IMember Getter { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IProperty"/> has a public
        /// <see cref="Setter"/>.
        /// </summary>
        bool IsWritable { get; }

        /// <summary>
        /// Gets an <see cref="IMember"/> describing the <see cref="IProperty"/>'s setter.
        /// </summary>
        IMember Setter { get; }
    }
}
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
        /// Gets an <see cref="IMethod"/> describing the <see cref="IProperty"/>'s getter.
        /// </summary>
        IMethod Getter { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IProperty"/> has a public
        /// <see cref="Setter"/>.
        /// </summary>
        bool IsWritable { get; }

        /// <summary>
        /// Gets an <see cref="IMethod"/> describing the <see cref="IProperty"/>'s setter.
        /// </summary>
        IMethod Setter { get; }
    }
}
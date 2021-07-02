namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    /// <summary>
    /// Implementing classes will provide metadata about a class or method open generic parameter.
    /// </summary>
    public interface IGenericParameter : IType
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericParameter"/> is constrained.
        /// </summary>
        bool HasConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericParameter"/> is constrained to
        /// reference types.
        /// </summary>
        bool HasClassConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericParameter"/> is constrained to
        /// value types.
        /// </summary>
        bool HasStructConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericParameter"/> is constrained to
        /// types with a public, parameterless constructor.
        /// </summary>
        bool HasNewableConstraint { get; }
    }
}
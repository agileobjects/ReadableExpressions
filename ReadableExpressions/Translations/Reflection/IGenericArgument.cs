namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide metadata about a method generic argument.
    /// </summary>
    public interface IGenericArgument
    {
        /// <summary>
        /// Gets the Type of this <see cref="IGenericArgument"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets a value indicating whther this <see cref="IGenericArgument"/> is constrained.
        /// </summary>
        bool HasConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericArgument"/> is constrained to
        /// reference types.
        /// </summary>
        bool HasClassConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericArgument"/> is constrained to
        /// value types.
        /// </summary>
        bool HasStructConstraint { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericArgument"/> is constrained to
        /// types with a public, parameterless constructor.
        /// </summary>
        bool HasNewableConstraint { get; }

        /// <summary>
        /// Gets this <see cref="IGenericArgument"/>'s collection of Type constraints, if any.
        /// </summary>
        ReadOnlyCollection<Type> TypeConstraints { get; }
    }
}
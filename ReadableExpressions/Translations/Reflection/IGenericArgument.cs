namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide metadata about a class or method generic argument.
    /// </summary>
    public interface IGenericArgument
    {
        /// <summary>
        /// Gets the Type of this <see cref="IGenericArgument"/>. If <see cref="IsClosed"/> returns
        /// false, this property should return typeof(Type).
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the name of the <see cref="Type"/> of this <see cref="IGenericArgument"/>. If
        /// <see cref="IsClosed"/> returns false, this property should return its generic parameter
        /// name.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IGenericArgument"/> has been closed, or
        /// represents an open generic parameter.
        /// </summary>
        bool IsClosed { get; }

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
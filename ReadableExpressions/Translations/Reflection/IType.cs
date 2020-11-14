namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;

    /// <summary>
    /// Implementing classes will provide metadata about a Type.
    /// </summary>
    public interface IType
    {
        /// <summary>
        /// Gets the namespace of this <see cref="IType"/>.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the name of this <see cref="IType"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the full name of this <see cref="IType"/>. If the Type is created at runtime,
        /// returns null.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an interface.
        /// </summary>
        bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents a class.
        /// </summary>
        bool IsClass { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents a primitive.
        /// </summary>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> is an anonymous Type.
        /// </summary>
        bool IsAnonymous { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> is an IEnumerable Type.
        /// </summary>
        bool IsEnumerable { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> is an IDictionary Type.
        /// </summary>
        bool IsDictionary { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> has generic parameters.
        /// </summary>
        bool IsGeneric { get; }

        /// <summary>
        /// Gets the number of generic parameters belonging to this <see cref="IType"/>, if this
        /// Type is generic.
        /// </summary>
        int GenericParameterCount { get; }

        /// <summary>
        /// Gets the <see cref="IType"/>s representing this Type's generic type arguments, if this
        /// type is generic. If this Type is not generic, returns an empty array.
        /// </summary>
        IType[] GenericTypeArguments { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> is nested within another Type.
        /// </summary>
        bool IsNested { get; }

        /// <summary>
        /// Gets the <see cref="IType"/> which declares this Type, if this Type is nested.
        /// </summary>
        IType DeclaringType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an array.
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// Gets the <see cref="IType"/> representing this <see cref="IType"/>'s elements, if this
        /// <see cref="IType"/> is an array or enumerable.
        /// </summary>
        IType ElementType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents System.Object.
        /// </summary>
        bool IsObjectType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> is a Nullable{T}.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        /// Gets the non-nullable <see cref="IType"/> underlying this Type, if this Type is a
        /// Nullable{T}.
        /// </summary>
        IType NonNullableUnderlyingType { get; }

        /// <summary>
        /// Gets the Type represented by this <see cref="IType"/>.
        /// </summary>
        /// <returns>The Type represented by this <see cref="IType"/>.</returns>
        Type AsType();
    }
}
namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    /// <summary>
    /// Implementing classes will provide metadata about a Type.
    /// </summary>
    public interface IType
    {
        /// <summary>
        /// Gets the Assembly to which this <see cref="IType"/> belongs.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        /// Gets the namespace of this <see cref="IType"/>.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the base <see cref="IType"/> of this Type, if any.
        /// </summary>
        IType BaseType { get; }

        /// <summary>
        /// Gets a set of <see cref="IType"/>s representing all the interfaces implemented by this
        /// <see cref="IType"/>, if any.
        /// </summary>
        ReadOnlyCollection<IType> AllInterfaces { get; }

        /// <summary>
        /// Gets the full name of this <see cref="IType"/>. If the Type is created at runtime,
        /// returns null.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the name of this <see cref="IType"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an interface.
        /// </summary>
        bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents a class.
        /// </summary>
        bool IsClass { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an enum.
        /// </summary>
        bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents a primitive.
        /// </summary>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an anonymous Type.
        /// </summary>
        bool IsAnonymous { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an abstract Type.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents a sealed Type.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an IEnumerable Type.
        /// </summary>
        bool IsEnumerable { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an IDictionary Type.
        /// </summary>
        bool IsDictionary { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> has generic parameters.
        /// </summary>
        bool IsGeneric { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> has open generic parameters.
        /// </summary>
        bool IsGenericDefinition { get; }

        /// <summary>
        /// Gets the <see cref="IType"/> describing this <see cref="IType"/>'s generic definition,
        /// or null if none exists.
        /// </summary>
        IType GenericDefinition { get; }

        /// <summary>
        /// Gets the number of generic parameters belonging to this <see cref="IType"/>, if this
        /// Type is generic.
        /// </summary>
        int GenericParameterCount { get; }

        /// <summary>
        /// Gets the <see cref="IType"/>s representing this Type's generic type arguments, if this
        /// type is generic. If this Type is not generic, returns an empty array.
        /// </summary>
        ReadOnlyCollection<IType> GenericTypeArguments { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IType"/> represents an open generic
        /// parameter Type.
        /// </summary>
        bool IsGenericParameter { get; }

        /// <summary>
        /// Gets the GenericParameterAttributes describing this <see cref="IType"/>'s constraints,
        /// if this IType represents an open generic parameter type.
        /// </summary>
        GenericParameterAttributes Constraints { get; }

        /// <summary>
        /// Gets the <see cref="IType"/>s to which this Type is constrained, if this IType represents
        /// an open generic parameter Type.
        /// </summary>
        ReadOnlyCollection<IType> ConstraintTypes { get; }

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
        /// <see cref="IType"/> represents an array, enumerable or ByRef Type.
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
        /// Gets a value indicating whether this <see cref="IType"/> is passed by reference.
        /// </summary>
        bool IsByRef { get; }

        /// <summary>
        /// Gets all the <see cref="IMember"/>s belonging to this <see cref="IType"/>.
        /// </summary>
        IEnumerable<IMember> AllMembers { get; }

        /// <summary>
        /// Returns this <see cref="IType"/>'s <see cref="IMember"/>s which match the criteria
        /// configured by the given <paramref name="selectionConfigurator"/>.
        /// </summary>
        /// <param name="selectionConfigurator">
        /// An Action setting up member selection criteria on a <see cref="MemberSelector"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IMember"/>s belonging to this <see cref="IType"/> which match the given
        /// <paramref name="selectionConfigurator"/>.
        /// </returns>
        IEnumerable<IMember> GetMembers(Action<MemberSelector> selectionConfigurator);

        /// <summary>
        /// Returns this <see cref="IType"/>'s <typeparamref name="TMember"/>s which match the
        /// criteria configured by the given <paramref name="selectionConfigurator"/>.
        /// </summary>
        /// <typeparam name="TMember">
        /// The <see cref="IMember"/> Type of the members to return.
        /// </typeparam>
        /// <param name="selectionConfigurator">
        /// An Action setting up member selection criteria on a <see cref="MemberSelector"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IMember"/>s belonging to this <see cref="IType"/> which match the given
        /// <paramref name="selectionConfigurator"/>.
        /// </returns>
        IEnumerable<TMember> GetMembers<TMember>(Action<MemberSelector> selectionConfigurator)
            where TMember : IMember;

        /// <summary>
        /// Returns a value indicating whether this <see cref="IType"/> represents the same Type as
        /// the given <paramref name="otherType"/>.
        /// </summary>
        /// <param name="otherType">The <see cref="IType"/> for which to make the determination.</param>
        /// <returns>
        /// True if this <see cref="IType"/> represents the same Type as the given
        /// <paramref name="otherType"/>, otherwise false.
        /// </returns>
        bool Equals(IType otherType);

        /// <summary>
        /// Gets the Type represented by this <see cref="IType"/>.
        /// </summary>
        /// <returns>The Type represented by this <see cref="IType"/>.</returns>
        Type AsType();
    }
}
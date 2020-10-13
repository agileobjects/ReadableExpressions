namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide metadata about a method.
    /// </summary>
    public interface IMethod
    {
        /// <summary>
        /// Gets the Type which declares this <see cref="IMethod"/>.
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is public.
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is protected internal.
        /// </summary>
        bool IsProtectedInternal { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is internal.
        /// </summary>
        bool IsInternal { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is protected.
        /// </summary>
        bool IsProtected { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is private.
        /// </summary>
        bool IsPrivate { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is static.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is virtual.
        /// </summary>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets the name of this <see cref="IMethod"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> has generic Type parameters.
        /// </summary>
        bool IsGenericMethod { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> is an extension method.
        /// </summary>
        bool IsExtensionMethod { get; }

        /// <summary>
        /// Gets an <see cref="IMethod"/> describing this <see cref="IMethod"/>'s generic method
        /// definition, or null if this <see cref="IMethod"/> is non-generic.
        /// </summary>
        IMethod GetGenericMethodDefinition();

        /// <summary>
        /// Gets <see cref="IGenericArgument"/>s describing this <see cref="IMethod"/>'s generic
        /// arguments, or an empty collection if <see cref="IsGenericMethod"/> is false.
        /// </summary>
        ReadOnlyCollection<IGenericArgument> GetGenericArguments();

        /// <summary>
        /// Gets this <see cref="IMethod"/>'s parameters, as an <see cref="IParameter"/> collection.
        /// </summary>
        ReadOnlyCollection<IParameter> GetParameters();

        /// <summary>
        /// Gets the return Type of this <see cref="IMethod"/>.
        /// </summary>
        Type ReturnType { get; }
    }
}
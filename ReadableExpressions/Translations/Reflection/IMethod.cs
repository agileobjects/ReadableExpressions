namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide metadata about a method.
    /// </summary>
    public interface IMethod : IComplexMember
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IMethod"/> has generic Type parameters.
        /// </summary>
        bool IsGenericMethod { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMethod"/> is an extension method.
        /// </summary>
        bool IsExtensionMethod { get; }

        /// <summary>
        /// Gets an <see cref="IMethod"/> describing the <see cref="IMethod"/>'s generic method
        /// definition, or null if the <see cref="IMethod"/> is non-generic.
        /// </summary>
        IMethod GetGenericMethodDefinition();

        /// <summary>
        /// Gets <see cref="IGenericArgument"/>s describing the <see cref="IMethod"/>'s generic
        /// arguments, or an empty collection if <see cref="IsGenericMethod"/> is false.
        /// </summary>
        ReadOnlyCollection<IGenericArgument> GetGenericArguments();

        /// <summary>
        /// Gets the <see cref="IMethod"/>'s parameters, as an <see cref="IParameter"/> collection.
        /// </summary>
        ReadOnlyCollection<IParameter> GetParameters();

        /// <summary>
        /// Gets the return Type of the <see cref="IMethod"/>.
        /// </summary>
        Type ReturnType { get; }
    }
}
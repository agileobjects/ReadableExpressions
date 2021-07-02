namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide metadata about a method.
    /// </summary>
    public interface IMethod : IMethodBase
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IMethod"/> has generic Type parameters.
        /// </summary>
        bool IsGenericMethod { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMethod"/> has a body.
        /// </summary>
        bool HasBody { get; }

        /// <summary>
        /// Gets an <see cref="IMethod"/> describing the <see cref="IMethod"/>'s generic method
        /// definition, or null if the <see cref="IMethod"/> is non-generic.
        /// </summary>
        IMethod GetGenericMethodDefinition();

        /// <summary>
        /// Gets <see cref="IGenericParameter"/>s describing the <see cref="IMethod"/>'s generic
        /// arguments, or an empty collection if <see cref="IsGenericMethod"/> is false.
        /// </summary>
        ReadOnlyCollection<IGenericParameter> GetGenericArguments();

        /// <summary>
        /// Gets the return <see cref="IType"/> of the <see cref="IMethod"/>.
        /// </summary>
        IType ReturnType { get; }
    }
}
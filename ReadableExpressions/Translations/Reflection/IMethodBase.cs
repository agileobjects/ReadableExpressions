namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Implementing classes will provide basic metadata about a method.
    /// </summary>
    public interface IMethodBase : IComplexMember
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="IMethodBase"/> is an extension method.
        /// </summary>
        bool IsExtensionMethod { get; }

        /// <summary>
        /// Gets the <see cref="IMethodBase"/>'s parameters, as an <see cref="IParameter"/>
        /// collection.
        /// </summary>
        ReadOnlyCollection<IParameter> GetParameters();
    }
}
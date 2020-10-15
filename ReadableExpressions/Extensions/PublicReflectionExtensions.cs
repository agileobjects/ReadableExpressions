namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Translations.Reflection;

    /// <summary>
    /// Provides extension methods to use with BCL reflection objects.
    /// </summary>
    public static class PublicReflectionExtensions
    {
        /// <summary>
        /// Gets a collection of <see cref="IGenericArgument"/>s describing this
        /// <paramref name="method"/>'s generic arguments, or an empty collection if this method is
        /// not generic.
        /// </summary>
        /// <param name="method">The MethodInfo for which to retrieve the <see cref="IGenericArgument"/>s.</param>
        /// <returns>
        /// A collection of <see cref="IGenericArgument"/>s describing this <paramref name="method"/>'s
        /// generic arguments, or an empty collection if this method is not generic.
        /// </returns>
        public static ReadOnlyCollection<IGenericArgument> GetGenericArgs(this MethodBase method)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<IGenericArgument>.EmptyReadOnlyCollection;
            }

            return method
                .GetGenericArguments()
                .ProjectToArray(GenericArgumentFactory.For)
                .ToReadOnlyCollection();
        }
    }
}
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

        /// <summary>
        /// Gets a string indicating the accessibility (public, internal, etc.) of this
        /// <see cref="IMember"/>.
        /// </summary>
        /// <param name="member">The <see cref="IMember"/> for which to retrieve the accessibility.</param>
        /// <returns>
        /// Whichever is appropriate of 'public', 'internal', 'protected internal', 'protected',
        /// 'private', or an empty string.
        /// </returns>
        public static string GetAccessibility(this IMember member)
        {
            if (member.IsPublic)
            {
                return "public";
            }

            if (member.IsInternal)
            {
                return "internal";
            }

            if (member.IsProtected)
            {
                return "protected";
            }

            if (member.IsProtectedInternal)
            {
                return "protected internal";
            }

            if (member.IsPrivate)
            {
                return "private";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a string containing the modifiers (abstract, override, etc.) of this
        /// <see cref="IComplexMember"/>.
        /// </summary>
        /// <param name="member">The <see cref="IComplexMember"/> for which to retrieve the modifiers.</param>
        /// <returns>
        /// Whichever is appropriate of 'abstract', 'static', 'override', 'virtual', or an empty
        /// string.
        /// </returns>
        public static string GetModifiers(this IComplexMember member)
        {
            if (member.IsAbstract)
            {
                return "abstract";
            }

            if (member.IsStatic)
            {
                return "static";
            }

            if (member.IsOverride)
            {
                return "override";
            }

            if (member.IsVirtual)
            {
                return "virtual";
            }

            return string.Empty;
        }
    }
}
namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using NetStandardPolyfills;
    using Translations.Reflection;

    /// <summary>
    /// Provides extension methods to use with BCL reflection objects.
    /// </summary>
    public static class PublicReflectionExtensions
    {
        /// <summary>
        /// Gets one or both <see cref="IComplexMember"/>s describing this <see cref="IProperty"/>'s
        /// accessors.
        /// </summary>
        /// <param name="property">The <see cref="IProperty"/> for which to retrieve the accessors.</param>
        /// <returns>Whichever is available of this <see cref="IProperty"/>'s getter and setter.</returns>
        public static IEnumerable<IComplexMember> GetAccessors(this IProperty property)
        {
            var accessors = new List<IComplexMember>(2);

            if (property.Getter != null)
            {
                accessors.Add(property.Getter);
            }

            if (property.Setter != null)
            {
                accessors.Add(property.Setter);
            }

            return accessors;
        }

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
        /// Gets a value indicating whether this <paramref name="member"/> belongs to an interface.
        /// This method checks for the <see cref="IPotentialInterfaceMember"/> interface, falling
        /// back to member.DeclaringType.IsInterface if it is not implemented.
        /// </summary>
        /// <param name="member">The <see cref="IMember"/> for which to make the determination.</param>
        /// <returns>True if this <paramref name="member"/> belongs to an interface, otherwise false.</returns>
        public static bool IsInterfaceMember(this IMember member)
        {
            if (member is IPotentialInterfaceMember interfaceMember)
            {
                return interfaceMember.IsInterfaceMember;
            }

            return member.DeclaringType.IsInterface();
        }

        /// <summary>
        /// Gets a string indicating the accessibility (public, internal, etc.) of this
        /// <see cref="IMember"/>. Returns an empty string if this member belongs to an interface.
        /// </summary>
        /// <param name="member">The <see cref="IMember"/> for which to retrieve the accessibility.</param>
        /// <returns>
        /// Whichever is appropriate of 'public', 'internal', 'protected internal', 'protected',
        /// 'private', or an empty string.
        /// </returns>
        public static string GetAccessibility(this IMember member)
        {
            if (member.IsInterfaceMember())
            {
                return string.Empty;
            }

            if (member.IsPublic)
            {
                return "public";
            }

            if (member.IsProtectedInternal)
            {
                return "protected internal";
            }

            if (member.IsInternal)
            {
                return "internal";
            }

            if (member.IsProtected)
            {
                return "protected";
            }

            if (member.IsPrivateProtected)
            {
                return "private protected";
            }

            if (member.IsPrivate)
            {
                return "private";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a string containing the modifiers (abstract, override, etc.) of this
        /// <see cref="IComplexMember"/>. Returns an empty string if this member belongs to an
        /// interface.
        /// </summary>
        /// <param name="member">The <see cref="IComplexMember"/> for which to retrieve the modifiers.</param>
        /// <returns>
        /// Whichever is appropriate of 'abstract', 'static', 'override', 'virtual', or an empty
        /// string.
        /// </returns>
        public static string GetModifiers(this IComplexMember member)
        {
            if (member.IsInterfaceMember())
            {
                return string.Empty;
            }

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
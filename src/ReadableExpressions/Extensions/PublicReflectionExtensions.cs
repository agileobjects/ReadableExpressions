namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;
    using Translations.Reflection;
    using static TranslationSettings;

    /// <summary>
    /// Provides extension methods to use with BCL reflection objects.
    /// </summary>
    public static class PublicReflectionExtensions
    {
        /// <summary>
        /// Gets all of this <paramref name="type"/>'s fields, properties and methods as
        /// <see cref="IMember"/> objects.
        /// </summary>
        /// <param name="type">The Type for which to retrieve the members.</param>
        /// <returns>
        /// This <paramref name="type"/>'s fields, properties and methods as <see cref="IMember"/>
        /// objects.
        /// </returns>
        public static IEnumerable<IMember> GetAllMembers(this Type type)
        {
            return type.GetPublicInstanceMembers()
                .Concat(type.GetPublicStaticMembers())
                .Concat(type.GetNonPublicInstanceMembers())
                .Concat(type.GetNonPublicStaticMembers())
                .Project(member =>
                {
                    return member switch
                    {
                        FieldInfo field => new ClrFieldWrapper(field),
                        PropertyInfo property => new ClrPropertyWrapper(property, Default),
                        MethodInfo method => new ClrMethodWrapper(method, Default),
                        _ => default(IMember)
                    };
                })
                .Filter(m => m != null)
                .ToList();
        }

        /// <summary>
        /// Gets one or both <see cref="IMethod"/>s describing this <see cref="IProperty"/>'s
        /// accessors.
        /// </summary>
        /// <param name="property">The <see cref="IProperty"/> for which to retrieve the accessors.</param>
        /// <returns>Whichever is available of this <see cref="IProperty"/>'s getter and setter.</returns>
        public static IEnumerable<IMethod> GetAccessors(this IProperty property)
        {
            var accessors = new List<IMethod>(2);

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
        /// Gets a collection of <see cref="IGenericParameter"/>s describing this
        /// <paramref name="method"/>'s generic arguments, or an empty collection if this method is
        /// not generic.
        /// </summary>
        /// <param name="method">The MethodInfo for which to retrieve the <see cref="IGenericParameter"/>s.</param>
        /// <returns>
        /// A collection of <see cref="IGenericParameter"/>s describing this <paramref name="method"/>'s
        /// generic arguments, or an empty collection if this method is not generic.
        /// </returns>
        public static ReadOnlyCollection<IGenericParameter> GetGenericArgs(this MethodBase method)
        {
            if (!method.IsGenericMethod)
            {
                return Enumerable<IGenericParameter>.EmptyReadOnlyCollection;
            }

            return method
                .GetGenericArguments()
                .ProjectToArray(GenericParameterFactory.For)
                .ToReadOnlyCollection();
        }

        /// <summary>
        /// Gets a value indicating whether this <paramref name="member"/> belongs to an interface.
        /// </summary>
        /// <param name="member">The <see cref="IMember"/> for which to make the determination.</param>
        /// <returns>True if this <paramref name="member"/> belongs to an interface, otherwise false.</returns>
        public static bool IsInterfaceMember(this IMember member)
            => member.DeclaringType.IsInterface;

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
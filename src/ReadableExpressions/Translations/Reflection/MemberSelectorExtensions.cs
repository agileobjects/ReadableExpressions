namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides <see cref="MemberSelector"/> extension methods.
    /// </summary>
    public static class MemberSelectorExtensions
    {
        /// <summary>
        /// Filters the given <paramref name="allMembers"/> to those matching the criteria defined
        /// by the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="allMembers">The <see cref="IMember"/> set to filter.</param>
        /// <param name="configuration">
        /// An Action setting up member selection criteria on a <see cref="MemberSelector"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IMember"/>s of the given <paramref name="allMembers"/> collection which
        /// match the criteria defined by the given <paramref name="configuration"/>.
        /// </returns>
        public static IEnumerable<IMember> Select(
            this IEnumerable<IMember> allMembers,
            Action<MemberSelector> configuration)
        {
            var selector = new MemberSelector(configuration);

            foreach (var member in allMembers)
            {
                if (selector.Include(member))
                {
                    yield return member;
                }
            }
        }
    }
}
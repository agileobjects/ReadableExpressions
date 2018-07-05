#if NET35
namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class InternalEnumerableExtensions
    {
        public static bool Contains<TContained, TItem>(this ICollection<TContained> items, TItem item)
            where TContained : TItem
        {
            return items.Any(i => i.Equals(item));
        }
    }
}
#endif
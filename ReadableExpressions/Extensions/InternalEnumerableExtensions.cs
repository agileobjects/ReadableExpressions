namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
#if NET35
    using System.Linq;
#endif

    internal static class InternalEnumerableExtensions
    {
        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, TResult> projector)
        {
            foreach (var item in items)
            {
                yield return projector.Invoke(item);
            }
        }

        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, int, TResult> projector)
        {
            var i = 0;

            foreach (var item in items)
            {
                yield return projector.Invoke(item, i++);
            }
        }

        public static IEnumerable<T> Combine<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (var item in first)
            {
                yield return item;
            }

            foreach (var item in second)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            foreach (var item in items)
            {
                if (predicate.Invoke(item))
                {
                    yield return item;
                }
            }
        }

#if NET35
        public static bool Contains<TContained, TItem>(this ICollection<TContained> items, TItem item)
            where TContained : TItem
        {
            return items.Any(i => i.Equals(item));
        }
#endif
    }
}
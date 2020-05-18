namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
#if NET35
    using System.Linq;
#endif

    internal static class InternalEnumerableExtensions
    {
        [DebuggerStepThrough]
        public static bool Any<T>(this ICollection<T> items) => items.Count > 0;

        public static IList<TResult> ProjectToArray<TItem, TResult>(this IList<TItem> items, Func<TItem, TResult> projector)
        {
            var itemCount = items.Count;
            var result = new TResult[itemCount];

            for (var i = 0; i < itemCount; ++i)
            {
                result[i] = projector.Invoke(items[i]);
            }

            return result;
        }

        public static IList<TResult> ProjectToArray<TItem, TResult>(this IList<TItem> items, Func<TItem, int, TResult> projector)
        {
            var itemCount = items.Count;
            var result = new TResult[itemCount];

            for (var i = 0; i < itemCount; ++i)
            {
                result[i] = projector.Invoke(items[i], i);
            }

            return result;
        }

        [DebuggerStepThrough]
        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, TResult> projector)
        {
            foreach (var item in items)
            {
                yield return projector.Invoke(item);
            }
        }

        [DebuggerStepThrough]
        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, int, TResult> projector)
        {
            var i = 0;

            foreach (var item in items)
            {
                yield return projector.Invoke(item, i++);
            }
        }

        [DebuggerStepThrough]
        public static IList<T> Combine<T>(this ICollection<T> first, IList<T> second)
        {
            var secondCount = second.Count;
            var combined = new T[first.Count + secondCount];
            var index = 0;

            foreach (var item in first)
            {
                combined[index] = item;

                ++index;
            }

            for (var i = 0; i < secondCount;)
            {
                combined[index] = second[i];
                
                ++index;
                ++i;
            }

            return combined;
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static T First<T>(this IList<T> items) => items[0];

        [DebuggerStepThrough]
        public static T Last<T>(this IList<T> items) => items[items.Count - 1];

        [DebuggerStepThrough]
        public static T FirstOrDefault<T>(this IList<T> items, Func<T, bool> predicate)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (predicate.Invoke(item))
                {
                    return item;
                }
            }

            return default(T);
        }
#if NET35
        [DebuggerStepThrough]
        public static bool Contains<TContained, TItem>(this ICollection<TContained> items, TItem item)
            where TContained : TItem
        {
            return items.Any(i => i.Equals(item));
        }
#endif
    }
}
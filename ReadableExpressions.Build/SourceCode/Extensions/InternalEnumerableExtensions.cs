namespace AgileObjects.ReadableExpressions.Build.SourceCode.Extensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class InternalEnumerableExtensions
    {
#if FEATURE_READONLYDICTIONARY
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
#endif
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this T item)
            => new[] { item }.ToReadOnlyCollection();

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this List<T> items)
            => new ReadOnlyCollection<T>(items);

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this T[] items)
            => new ReadOnlyCollection<T>(items);
    }
}

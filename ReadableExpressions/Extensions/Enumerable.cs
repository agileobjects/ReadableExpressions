namespace AgileObjects.ReadableExpressions.Extensions
{
#if NETSTANDARD2_0
    using System;
#endif
    using System.Collections.ObjectModel;

    internal static class Enumerable<T>
    {
#if NETSTANDARD2_0
        public static readonly T[] EmptyArray = Array.Empty<T>();
#else
        public static readonly T[] EmptyArray = { };
#endif
        public static readonly ReadOnlyCollection<T> EmptyReadOnlyCollection =
            new ReadOnlyCollection<T>(EmptyArray);
    }
}
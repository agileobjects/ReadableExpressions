namespace AgileObjects.ReadableExpressions.Extensions
{
#if NETSTANDARD2_0
    using System;
#endif

    internal static class Enumerable<T>
    {
#if NETSTANDARD2_0
        public static readonly T[] EmptyArray = Array.Empty<T>();
#else
        public static readonly T[] EmptyArray = { };
#endif
    }
}
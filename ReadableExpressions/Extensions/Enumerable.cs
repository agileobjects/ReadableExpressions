namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class Enumerable<T>
    {
        public static readonly T[] EmptyArray = new T[0];

        public static readonly List<T> EmptyList = new List<T>(0);

        public static readonly ReadOnlyCollection<T> EmptyReadOnlyCollection =
            EmptyArray.ToReadOnlyCollection();
    }
}
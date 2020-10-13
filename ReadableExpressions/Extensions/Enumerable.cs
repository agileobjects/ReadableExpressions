namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.ObjectModel;

    internal static class Enumerable<T>
    {
        public static readonly T[] EmptyArray = { };

        public static readonly ReadOnlyCollection<T> EmptyReadOnlyCollection =
            new ReadOnlyCollection<T>(EmptyArray);
    }
}
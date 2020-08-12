namespace AgileObjects.ReadableExpressions.Build.SourceCode.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class Enumerable<T>
    {
        public static readonly T[] EmptyArray = Array.Empty<T>();

        public static readonly List<T> EmptyList = new List<T>();

        public static readonly ReadOnlyCollection<T> EmptyReadOnlyCollection =
            EmptyArray.ToReadOnlyCollection();
    }
}
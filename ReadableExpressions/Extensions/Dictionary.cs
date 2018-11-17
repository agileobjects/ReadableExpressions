namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Collections.Generic;

    internal static class EmptyDictionary<TKey, TValue>
    {
        public static Dictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>(0);
    }
}
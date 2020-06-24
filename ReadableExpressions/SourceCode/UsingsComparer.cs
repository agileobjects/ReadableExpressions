namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System.Collections.Generic;
    using static System.StringComparison;

    internal class UsingsComparer : IComparer<string>
    {
        public static readonly IComparer<string> Instance = new UsingsComparer();

        public int Compare(string x, string y)
        {
            // ReSharper disable once PossibleNullReferenceException
            if (x.StartsWith("System", Ordinal))
            {
                // ReSharper disable once PossibleNullReferenceException
                if (y.StartsWith("System", Ordinal))
                {
                    return Comparer<string>.Default.Compare(x, y);
                }

                return -1;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (y.StartsWith("System", Ordinal))
            {
                return 1;
            }

            return Comparer<string>.Default.Compare(x, y);
        }
    }
}

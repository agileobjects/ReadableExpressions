namespace AgileObjects.ReadableExpressions.SourceCode
{
    using Api;

    internal static class SourceCodeExpressionExtensions
    {
        public static string GetClassName(
            this SourceCodeExpression sourceCodeExpression,
            IClassNamingContext @class)
        {
            const string DEFAULT_NAME = "GeneratedExpressionClass";

            if (sourceCodeExpression == null)
            {
                return DEFAULT_NAME;
            }

            var sourceCodeClasses = sourceCodeExpression.Classes;

            if (sourceCodeClasses.Count == 1)
            {
                return DEFAULT_NAME;
            }

            var classIndex = sourceCodeClasses.IndexOf((ClassExpression)@class);

            return DEFAULT_NAME + (classIndex + 1);
        }
    }
}
namespace AgileObjects.ReadableExpressions.SourceCode
{
    internal static class SourceCodeExpressionExtensions
    {
        public static string GetClassName(
            this SourceCodeExpression sourceCodeExpression,
            ClassExpression @class)
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

            var classIndex = sourceCodeClasses.IndexOf(@class);

            return DEFAULT_NAME + (classIndex + 1);
        }
    }
}
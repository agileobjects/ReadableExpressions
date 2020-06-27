namespace AgileObjects.ReadableExpressions.SourceCode
{
    using Api;

    internal static class SourceCodeExpressionExtensions
    {
        public static string GetClassName(
            this SourceCodeExpression sourceCodeExpression,
            IClassNamingContext classCtx)
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

            var classIndex = sourceCodeClasses.IndexOf((ClassExpression)classCtx);

            return DEFAULT_NAME + (classIndex + 1);
        }
    }

    internal static class ClassExpressionExtensions
    {
        public static string GetMethodName(
            this ClassExpression classExpression,
            IMethodNamingContext methodCtx)
        {
            var defaultName = (methodCtx.ReturnType != typeof(void))
                ? "Get" + methodCtx.ReturnTypeName
                : "DoAction";

            if (classExpression == null)
            {
                return defaultName;
            }

            var classMethods = classExpression.Methods;

            if (classMethods.Count == 1)
            {
                return defaultName;
            }

            var classIndex = classMethods.IndexOf((MethodExpression)methodCtx);

            return defaultName + (classIndex + 1);
        }
    }
}
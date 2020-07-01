namespace AgileObjects.ReadableExpressions.SourceCode
{
    using Api;

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

            var classMethods = classExpression.MethodsByReturnType[methodCtx.ReturnType];

            if (classMethods.Count == 1)
            {
                return defaultName;
            }

            var classIndex = classMethods.IndexOf((MethodExpression)methodCtx);

            return defaultName + (classIndex + 1);
        }
    }
}
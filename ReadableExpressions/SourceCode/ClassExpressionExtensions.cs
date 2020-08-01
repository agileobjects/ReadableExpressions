namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Linq;
    using Api;

    internal static class SourceCodeNameingExtensions
    {
        public static string ThrowIfInvalidName<TException>(
            this string name,
            string symbolType,
            bool ignoreIfNull = false)
            where TException : Exception
        {
            if (name == null)
            {
                if (ignoreIfNull)
                {
                    return null;
                }

                throw Create<TException>(symbolType + " names cannot be null");
            }

            if (name.Trim() == string.Empty)
            {
                throw Create<TException>(symbolType + " names cannot be blank");
            }

            if (char.IsDigit(name[0]) ||
                name.ToCharArray().Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            {
                throw Create<TException>($"'{name}' is an invalid {symbolType.ToLowerInvariant()} name");
            }

            return name;
        }

        private static Exception Create<TException>(string message)
            => (Exception)Activator.CreateInstance(typeof(TException), message);
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
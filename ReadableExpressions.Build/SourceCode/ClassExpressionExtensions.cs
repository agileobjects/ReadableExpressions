namespace AgileObjects.ReadableExpressions.Build.SourceCode
{
    using System.Linq;
    using System.Reflection;
    using AgileObjects.ReadableExpressions.Extensions;
    using Api;
    using NetStandardPolyfills;

    internal static class ClassExpressionExtensions
    {
        public static string GetMethodName(
            this ClassExpression classExpression,
            IMethodNamingContext methodCtx)
        {
            if (classExpression == null)
            {
                return methodCtx.GetDefaultName();
            }

            var classMethods = classExpression.MethodsByReturnType[methodCtx.ReturnType];
            var method = (MethodExpression)methodCtx;

            if (classMethods.Count == 1)
            {
                return method.GetName();
            }

            var methodIndex = classMethods.IndexOf(method);

            return method.GetName() + (methodIndex + 1);
        }

        private static string GetDefaultName(this IMethodNamingContext methodCtx)
        {
            return (methodCtx.ReturnType != typeof(void))
                ? "Get" + methodCtx.ReturnTypeName
                : "DoAction";
        }

        private static string GetName(this MethodExpression method)
        {
            if (method.Parent.Interfaces.Count == 0)
            {
                return method.GetDefaultName();
            }

            var parameterTypes = method.Parameters.ProjectToArray(p => p.Type);
            var classInterfaces = method.Parent.Interfaces;

            var matchingInterfaceMethods = classInterfaces
                .SelectMany(type => new[] { type }
                    .Concat(type.GetAllInterfaces())
                    .SelectMany(it => it.GetPublicMethods()))
                .Filter(im =>
                    im.ReturnType == method.ReturnType &&
                    im.GetParameters().Project(p => p.ParameterType).SequenceEqual(parameterTypes))
                .ToList();

            switch (matchingInterfaceMethods.Count)
            {
                case 0:
                    return method.GetDefaultName();

                case 1:
                    return matchingInterfaceMethods[0].Name;

                default:
                    var argumentTypes = string.Join(", ",
                        method.Parameters.ProjectToArray(p => p.Type.GetFriendlyName()));

                    var returnType = method.ReturnType.GetFriendlyName();

                    throw new AmbiguousMatchException(
                        $"Method '({argumentTypes}): {returnType}' matches multiple interface methods");
            }
        }
    }
}
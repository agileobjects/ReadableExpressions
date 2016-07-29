namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal interface IMethodInfo
    {
        string Name { get; }

        bool IsGenericMethod { get; }

        bool IsExtensionMethod { get; }

        MethodInfo GetGenericMethodDefinition();

        IEnumerable<Type> GetGenericArguments();

        IEnumerable<ParameterInfo> GetParameters();

        Type GetGenericArgumentFor(Type parameterType);
    }
}
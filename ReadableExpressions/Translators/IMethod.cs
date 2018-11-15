namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Reflection;

    internal interface IMethod
    {
        string Name { get; }

        bool IsGenericMethod { get; }

        bool IsExtensionMethod { get; }

        MethodInfo GetGenericMethodDefinition();

        Type[] GetGenericArguments();

        ParameterInfo[] GetParameters();

        Type GetGenericArgumentFor(Type parameterType);

        Type ReturnType { get; }
    }
}
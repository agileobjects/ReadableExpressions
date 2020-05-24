namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    using System;
    using System.Reflection;

    internal interface IMethod
    {
        string Name { get; }

        bool IsGenericMethod { get; }

        bool IsExtensionMethod { get; }

        IMethod GetGenericMethodDefinition();

        Type[] GetGenericArguments();

        ParameterInfo[] GetParameters();

        Type ReturnType { get; }
    }
}
namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;

    internal interface IMethod
    {
        Type DeclaringType { get; }

        bool IsPublic { get; }

        bool IsProtectedInternal { get; }

        bool IsInternal { get; }

        bool IsProtected { get; }

        bool IsPrivate { get; }

        bool IsAbstract { get; }

        bool IsStatic { get; }

        bool IsVirtual { get; }

        string Name { get; }

        bool IsGenericMethod { get; }

        bool IsExtensionMethod { get; }

        IMethod GetGenericMethodDefinition();

        Type[] GetGenericArguments();

        IList<IParameter> GetParameters();

        Type ReturnType { get; }
    }
}
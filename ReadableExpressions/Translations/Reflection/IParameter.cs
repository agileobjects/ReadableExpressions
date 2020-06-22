namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;

    internal interface IParameter
    {
        Type Type { get; }
        
        string Name { get; }

        bool IsOut { get; }
        
        bool IsParamsArray { get; }
    }
}
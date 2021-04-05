namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;

    /// <summary>
    /// Implementing classes will describe a method parameter.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Gets the method parameter's type.
        /// </summary>
        IType Type { get; }
        
        /// <summary>
        /// Gets the method parameter's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the method parameter is an out parameter.
        /// </summary>
        bool IsOut { get; }
        
        /// <summary>
        /// Gets a value indicating whether the method parameter is a params array.
        /// </summary>
        bool IsParamsArray { get; }
    }
}
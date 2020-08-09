namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Implementing classes will translate an Expression to a source-code string.
    /// </summary>
    public interface ITranslation : ITranslatable
    {
        /// <summary>
        /// Gets the ExpressionType of the translated Expression.
        /// </summary>
        ExpressionType NodeType { get; }

        /// <summary>
        /// Gets the type of the translated Expression.
        /// </summary>
        Type Type { get; }
    }
}
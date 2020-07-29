namespace AgileObjects.ReadableExpressions.SourceCode.Translations.Interfaces
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using AgileObjects.ReadableExpressions.Translations.Interfaces;
    using SourceCode;

    internal interface ISourceCodeTranslationContext : ITranslationContext
    {
        /// <summary>
        /// Gets the namespaces required by the translated Expression.
        /// </summary>
        IList<string> RequiredNamespaces { get; }

        IList<ParameterExpression> GetUnscopedVariablesFor(MethodExpression method);
    }
}
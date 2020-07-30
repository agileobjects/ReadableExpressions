namespace AgileObjects.ReadableExpressions.SourceCode.Translations.Interfaces
{
    using System.Collections.Generic;
    using AgileObjects.ReadableExpressions.Translations.Interfaces;

    internal interface ISourceCodeTranslationContext : ITranslationContext
    {
        /// <summary>
        /// Gets the namespaces required by the translated Expression.
        /// </summary>
        IList<string> RequiredNamespaces { get; }
    }
}
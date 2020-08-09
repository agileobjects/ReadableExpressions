namespace AgileObjects.ReadableExpressions.Build.SourceCode.Translations.Interfaces
{
    using System.Collections.Generic;
    using ReadableExpressions.Translations.Interfaces;

    internal interface ISourceCodeTranslationContext : ITranslationContext
    {
        /// <summary>
        /// Gets the namespaces required by the translated Expression.
        /// </summary>
        IList<string> RequiredNamespaces { get; }
    }
}
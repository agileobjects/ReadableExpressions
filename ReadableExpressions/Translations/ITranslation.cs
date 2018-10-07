namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ITranslation
    {
        ExpressionType NodeType { get; }

        int EstimatedSize { get; }

        void WriteTo(ITranslationContext context);
    }
}
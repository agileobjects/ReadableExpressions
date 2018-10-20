namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class GotoTranslation : ITranslation
    {
        public GotoTranslation(GotoExpression @goto, ITranslationContext context)
        {

        }

        public ExpressionType NodeType => ExpressionType.Goto;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
        }
    }
}
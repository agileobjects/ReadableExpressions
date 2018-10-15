namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConditionTranslation : ITranslation
    {
        private readonly ITranslation _conditionTranslation;

        public ConditionTranslation(Expression condition, ITranslationContext context)
        {
            NodeType = condition.NodeType;
            _conditionTranslation = context.GetCodeBlockTranslationFor(condition);
            EstimatedSize = _conditionTranslation.EstimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _conditionTranslation.WriteTo(context);
        }
    }
}
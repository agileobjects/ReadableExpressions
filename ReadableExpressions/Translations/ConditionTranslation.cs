namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConditionTranslation : ITranslation
    {
        private readonly CodeBlockTranslation _conditionTranslation;

        public ConditionTranslation(Expression condition, ITranslationContext context)
        {
            NodeType = condition.NodeType;
            
            var conditionTranslation = context.GetTranslationFor(condition);
            var conditionCodeBlockTranslation = new CodeBlockTranslation(conditionTranslation);

            if (conditionTranslation.IsMultiStatement())
            {
                conditionCodeBlockTranslation.WithSingleLamdaParameterFormatting();
            }

            _conditionTranslation = conditionCodeBlockTranslation;
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
namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class NegationTranslation : ITranslation
    {
        private readonly char _operator;
        private readonly ITranslation _negatedValue;

        public NegationTranslation(UnaryExpression negation, ITranslationContext context)
        {
            NodeType = negation.NodeType;
            _operator = NodeType == ExpressionType.Not ? '!' : '-';
            _negatedValue = context.GetTranslationFor(negation.Operand);
            EstimatedSize = _negatedValue.EstimatedSize + 1;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation(_operator);
            _negatedValue.WriteTo(context);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class FixedValueTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly string _value;

        public FixedValueTranslation(Expression expression)
            : this(expression.NodeType, expression.ToString(), isTerminated: false)
        {
        }

        public FixedValueTranslation(ExpressionType expressionType, string terminatedValue)
            : this(expressionType, terminatedValue, isTerminated: true)
        {
        }

        private FixedValueTranslation(ExpressionType expressionType, string value, bool isTerminated)
        {
            NodeType = expressionType;
            _value = value;
            IsTerminated = isTerminated;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize => _value.Length;

        public bool IsTerminated { get; }

        public void WriteTo(ITranslationContext context) => context.WriteToTranslation(_value);
    }
}
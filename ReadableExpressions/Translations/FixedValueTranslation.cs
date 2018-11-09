namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class FixedValueTranslation : ITranslation
    {
        private readonly string _value;

        public FixedValueTranslation(Expression expression)
            : this(expression.NodeType, expression.ToString())
        {
        }

        public FixedValueTranslation(ExpressionType expressionType, string value)
        {
            NodeType = expressionType;
            _value = value;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize => _value.Length;

        public void WriteTo(ITranslationContext context) => context.WriteToTranslation(_value);
    }
}
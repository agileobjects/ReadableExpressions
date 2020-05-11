namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Formatting;
    using Interfaces;

    internal class FixedValueTranslation : ITranslation
    {
        private readonly string _value;
        private readonly TokenType _tokenType;

        public FixedValueTranslation(Expression expression)
            : this(
                expression.NodeType,
                expression.ToString(),
                expression.Type,
                TokenType.Default)
        {
        }

        public FixedValueTranslation(
            ExpressionType expressionType,
            string value,
            Type type,
            TokenType tokenType)
        {
            NodeType = expressionType;
            Type = type;
            _value = value;
            _tokenType = tokenType;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int TranslationSize => _value.Length;
        
        public int FormattingSize => 0;

        public int GetLineCount() => _value.GetLineCount();

        public void WriteTo(TranslationBuffer buffer)
            => buffer.WriteToTranslation(_value, _tokenType);
    }
}
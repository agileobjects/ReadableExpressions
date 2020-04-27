namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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

        public int EstimatedSize => _value.Length;

        public void WriteTo(TranslationBuffer buffer)
            => buffer.WriteToTranslation(_value, _tokenType);
    }

    internal class FixedValuesTranslation : ITranslation
    {
        private readonly IEnumerable<Action<TranslationBuffer>> _valueWriters;

        public FixedValuesTranslation(
            ExpressionType nodeType,
            Type type,
            int estimatedSize,
            params Action<TranslationBuffer>[] valueWriters)
        {
            NodeType = nodeType;
            Type = type;
            EstimatedSize = estimatedSize;
            _valueWriters = valueWriters;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            foreach (var valueWriter in _valueWriters)
            {
                valueWriter.Invoke(buffer);
            }
        }
    }
}
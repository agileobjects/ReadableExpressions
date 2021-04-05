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

    internal class FixedValueTranslation : ITranslation
    {
        private readonly string _value;
        private readonly TokenType _tokenType;

        public FixedValueTranslation(Expression expression, ITranslationContext context)
            : this(
                expression.NodeType,
                expression.ToString(),
                expression.Type,
                TokenType.Default,
                context)
        {
        }

        public FixedValueTranslation(
            ExpressionType expressionType,
            string value,
            Type type,
            TokenType tokenType,
            ITranslationContext context)
        {
            NodeType = expressionType;
            Type = type;
            _value = value;
            _tokenType = tokenType;
            FormattingSize = context.GetFormattingSize(tokenType);
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int TranslationSize => _value.Length;
        
        public int FormattingSize { get; }

        public int GetIndentSize() => 0;

        public int GetLineCount() => _value.GetLineCount();

        public void WriteTo(TranslationWriter writer)
            => writer.WriteToTranslation(_value, _tokenType);
    }
}
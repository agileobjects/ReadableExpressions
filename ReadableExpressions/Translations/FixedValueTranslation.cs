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

    /// <summary>
    /// An <see cref="ITranslation"/> with a fixed string value.
    /// </summary>
    public class FixedValueTranslation : ITranslation
    {
        private readonly string _value;
        private readonly TokenType _tokenType;
        private readonly int _formattingSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedValueTranslation"/> class.
        /// </summary>
        /// <param name="expression">The Expression translated by the <see cref="FixedValueTranslation"/>.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the Expression translation taking place.
        /// </param>
        public FixedValueTranslation(Expression expression, ITranslationContext context)
            : this(
                expression.NodeType,
                expression.ToString(),
                expression.Type,
                TokenType.Default,
                context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedValueTranslation"/> class.
        /// </summary>
        /// <param name="expressionType">The ExpressionType of the <see cref="FixedValueTranslation"/>.</param>
        /// <param name="value">The fixed, translated value.</param>
        /// <param name="type">The type of the <see cref="FixedValueTranslation"/>.</param>
        /// <param name="tokenType">
        /// The <see cref="TokenType"/> with which the <see cref="FixedValueTranslation"/> should be
        /// written to the translation output.
        /// </param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the current Expression translation.
        /// </param>
        public FixedValueTranslation(
            ExpressionType expressionType,
            string value,
            Type type,
            TokenType tokenType,
            ITranslationContext context)
            : this(expressionType, value, type, tokenType, context.Settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedValueTranslation"/> class.
        /// </summary>
        /// <param name="expressionType">The ExpressionType of the <see cref="FixedValueTranslation"/>.</param>
        /// <param name="value">The fixed, translated value.</param>
        /// <param name="type">The type of the <see cref="FixedValueTranslation"/>.</param>
        /// <param name="tokenType">
        /// The <see cref="TokenType"/> with which the <see cref="FixedValueTranslation"/> should be
        /// written to the translation output.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public FixedValueTranslation(
            ExpressionType expressionType,
            string value,
            Type type,
            TokenType tokenType,
            TranslationSettings settings)
        {
            NodeType = expressionType;
            Type = type;
            _value = value;
            _tokenType = tokenType;
            _formattingSize = settings.GetFormattingSize(tokenType);
        }

        /// <summary>
        /// Gets the ExpressionType of this <see cref="FixedValueTranslation"/>.
        /// </summary>
        public ExpressionType NodeType { get; }

        /// <summary>
        /// Gets the type of this <see cref="FixedValueTranslation"/>.
        /// </summary>
        public Type Type { get; }

        int ITranslatable.TranslationSize => _value.Length;

        int ITranslatable.FormattingSize => _formattingSize;

        int ITranslatable.GetIndentSize() => 0;

        int ITranslatable.GetLineCount() => _value.GetLineCount();

        void ITranslatable.WriteTo(TranslationWriter writer)
            => writer.WriteToTranslation(_value, _tokenType);
    }
}
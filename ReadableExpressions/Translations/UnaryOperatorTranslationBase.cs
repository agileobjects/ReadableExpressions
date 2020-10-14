namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    /// <summary>
    /// An <see cref="ITranslation"/> which writes a use of an operator.
    /// </summary>
    public abstract class UnaryOperatorTranslationBase : ITranslation
    {
        private readonly string _operator;
        private readonly ITranslatable _operandTranslation;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperatorTranslationBase"/> class.
        /// </summary>
        /// <param name="operator">The name of the operator being applied.</param>
        /// <param name="operandTranslation">
        /// The <see cref="ITranslatable"/> which will write the symbol to which the operator is
        /// being applied.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        protected UnaryOperatorTranslationBase(
            string @operator,
            ITranslatable operandTranslation,
            TranslationSettings settings)
        {
            _operator = @operator;
            _operandTranslation = operandTranslation;
            TranslationSize = @operator.Length + "()".Length + operandTranslation.TranslationSize;
            FormattingSize = settings.GetKeywordFormattingSize() + operandTranslation.FormattingSize;
        }

        /// <summary>
        /// Gets the ExpressionType of the translated operator use - ExpressionType.Extension.
        /// </summary>
        public ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        /// When overridden in a derived class, gets the type of the operand use represented by this
        /// <see cref="UnaryOperatorTranslationBase"/>.
        /// </summary>
        public abstract Type Type { get; }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize()
            => _operandTranslation.GetIndentSize();

        /// <inheritdoc />
        public int GetLineCount()
            => _operandTranslation.GetLineCount();

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_operator);
            writer.WriteToTranslation('(');
            _operandTranslation.WriteTo(writer);
            writer.WriteToTranslation(')');
        }
    }
}
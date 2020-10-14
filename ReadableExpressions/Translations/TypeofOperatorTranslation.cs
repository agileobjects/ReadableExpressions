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
    /// An <see cref="ITranslation"/> which writes a use of the typeof operator.
    /// </summary>
    public class TypeofOperatorTranslation : ITranslation
    {
        private readonly ITranslation _typeNameTranslation;
        private readonly string _typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeofOperatorTranslation"/> class.
        /// </summary>
        /// <param name="type">The Type to which the typeof operator is being applied.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public TypeofOperatorTranslation(Type type, TranslationSettings settings)
        {
            _typeNameTranslation = new TypeNameTranslation(type, settings);
            TranslationSize = GetTranslationSize(_typeNameTranslation.TranslationSize);
            FormattingSize = GetFormattingSize(_typeNameTranslation.FormattingSize, settings);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeofOperatorTranslation"/> class.
        /// </summary>
        /// <param name="typeName">The name of the Type to which the typeof operator is being applied.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public TypeofOperatorTranslation(string typeName, TranslationSettings settings)
        {
            _typeName = typeName;
            TranslationSize = GetTranslationSize(typeName.Length);
            FormattingSize = GetFormattingSize(settings.GetTypeNameFormattingSize(), settings);
        }

        #region Setup

        private static int GetTranslationSize(int typeNameTranslationSize)
            => typeNameTranslationSize + "typeof()".Length;

        private static int GetFormattingSize(int typeNameFormattingSize, TranslationSettings settings)
            => settings.GetKeywordFormattingSize() + typeNameFormattingSize;

        #endregion

        /// <summary>
        /// Gets the ExpressionType of the translated typeof operation - ExpressionType.Constant.
        /// </summary>
        public ExpressionType NodeType => ExpressionType.Constant;

        /// <summary>
        /// Gets the type of this <see cref="TypeofOperatorTranslation"/>, which is 'Type'.
        /// </summary>
        public Type Type => typeof(Type);

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize()
            => _typeNameTranslation?.GetIndentSize() ?? 0;

        /// <inheritdoc />
        public int GetLineCount()
            => _typeNameTranslation?.GetLineCount() ?? 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation("typeof");

            if (_typeNameTranslation != null)
            {
                _typeNameTranslation.WriteInParentheses(writer);
                return;
            }

            writer.WriteToTranslation('(');
            writer.WriteTypeNameToTranslation(_typeName);
            writer.WriteToTranslation(')');
        }
    }
}
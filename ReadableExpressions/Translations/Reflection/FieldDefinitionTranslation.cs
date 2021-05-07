namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a field signature, including accessibility and scope.
    /// </summary>
    public class FieldDefinitionTranslation : ITranslation
    {
        private readonly IField _field;
        private readonly string _modifiers;
        private readonly ITranslatable _fieldTypeNameTranslation;
        private readonly string _fieldName;
        private readonly ITranslatable _declaringTypeNameTranslation;

        internal FieldDefinitionTranslation(
            FieldInfo field,
            TranslationSettings settings)
            : this(new ClrFieldWrapper(field), includeDeclaringType: true, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDefinitionTranslation"/> class.
        /// </summary>
        /// <param name="field">The <see cref="IField"/> to translate.</param>
        /// <param name="includeDeclaringType">
        /// A value indicating whether to include the <paramref name="field"/>'s declaring type
        /// in the translation.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public FieldDefinitionTranslation(
            IField field,
            bool includeDeclaringType,
            TranslationSettings settings)
        {
            _field = field;
            _modifiers = field.GetAccessibilityForTranslation();

            if (field.IsConstant)
            {
                _modifiers += "const ";
            }
            else
            {
                if (field.IsStatic)
                {
                    _modifiers += "static ";
                }

                if (field.IsReadonly)
                {
                    _modifiers += "readonly ";
                }
            }

            _fieldTypeNameTranslation =
                new TypeNameTranslation(field.Type, settings);

            _fieldName = field.Name;

            var translationSize =
                _modifiers.Length +
                _fieldTypeNameTranslation.TranslationSize +
                _fieldName.Length +
                1; // For terminating ;

            var formattingSize =
                 settings.GetKeywordFormattingSize() +
                _fieldTypeNameTranslation.FormattingSize;

            if (includeDeclaringType && field.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(field.DeclaringType, settings);

                translationSize += _declaringTypeNameTranslation.TranslationSize + ".".Length;
                formattingSize += _declaringTypeNameTranslation.FormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        /// <inheritdoc />
        public ExpressionType NodeType => ExpressionType.MemberAccess;

        /// <inheritdoc />
        public Type Type => _field.Type.AsType();

        /// <inheritdoc />
        public virtual int TranslationSize { get; }

        /// <inheritdoc />
        public virtual int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => 0;

        /// <inheritdoc />
        public int GetLineCount() => 1;

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            WriteDefinitionTo(writer);
            writer.WriteToTranslation(';');
        }

        /// <summary>
        /// Writes the field definition to the given <paramref name="writer"/>, without a terminating
        /// semi-colon.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the field definition.</param>
        public void WriteDefinitionTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_modifiers);

            _fieldTypeNameTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_fieldName);
        }
    }
}
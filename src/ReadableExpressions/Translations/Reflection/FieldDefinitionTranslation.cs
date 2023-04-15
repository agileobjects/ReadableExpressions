namespace AgileObjects.ReadableExpressions.Translations.Reflection;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions;

/// <summary>
/// An <see cref="ITranslation"/> for a field signature, including accessibility and scope.
/// </summary>
public class FieldDefinitionTranslation : INodeTranslation
{
    private readonly string _modifiers;
    private readonly ITranslation _fieldTypeNameTranslation;
    private readonly string _fieldName;
    private readonly ITranslation _declaringTypeNameTranslation;

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

        if (includeDeclaringType && field.DeclaringType != null)
        {
            _declaringTypeNameTranslation =
                new TypeNameTranslation(field.DeclaringType, settings);
        }
    }

    /// <inheritdoc />
    public ExpressionType NodeType => ExpressionType.MemberAccess;

    /// <inheritdoc />
    public virtual int TranslationLength
    {
        get
        {
            var translationLength =
                _modifiers.Length +
                _fieldTypeNameTranslation.TranslationLength +
                _fieldName.Length +
                ";".Length;
            
            if (_declaringTypeNameTranslation == null)
            {
                return translationLength;
            }

            return translationLength + 
                  _declaringTypeNameTranslation.TranslationLength + 
                  ".".Length;
        }
    }

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        WriteDefinitionTo(writer);
        writer.WriteSemiColonToTranslation();
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
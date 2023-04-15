namespace AgileObjects.ReadableExpressions.Translations;

using System;
using System.Linq;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;
using Reflection;

/// <summary>
/// An <see cref="INodeTranslation"/> to translate a Type name.
/// </summary>
public class TypeNameTranslation : INodeTranslation
{
    private const string _object = "object";
    private readonly IType _type;
    private readonly TranslationSettings _settings;
    private readonly bool _isObject;
    private bool _writeObjectTypeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNameTranslation"/> class for the given
    /// <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The Type the name of which should be translated.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public TypeNameTranslation(Type type, TranslationSettings settings)
        : this(ClrTypeWrapper.For(type), settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNameTranslation"/> class for the given
    /// <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="IType"/> the name of which should be translated.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public TypeNameTranslation(IType type, TranslationSettings settings)
    {
        _type = type;
        _settings = settings;
        _isObject = type.IsObjectType;
    }

    /// <inheritdoc />
    public ExpressionType NodeType => ExpressionType.Constant;

    /// <inheritdoc />
    public int TranslationLength
    {
        get
        {
            if (_isObject)
            {
                return _object.Length;
            }

            var translationLength = GetTypeNameLength(_type);

            if (_type.IsGeneric)
            {
                translationLength +=
                    "<>".Length +
                    _type.GenericTypeArguments.Sum(GetTypeNameLength);
            }

            return translationLength;
        }
    }

    private int GetTypeNameLength(IType type)
    {
        var translationSize = type.GetKeywordOrNull()?.Length ?? type.Name.Length;

        if (_settings.FullyQualifyTypeNames && type.Namespace != null)
        {
            translationSize += type.Namespace.Length;
        }

        while (type.IsNested)
        {
            type = type.DeclaringType;
            translationSize += GetTypeNameLength(type);
        }

        return translationSize;
    }

    internal TypeNameTranslation WithObjectTypeName()
    {
        if (_isObject)
        {
            _writeObjectTypeName = true;
        }

        return this;
    }

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        if (_isObject)
        {
            if (_writeObjectTypeName)
            {
                writer.WriteTypeNameToTranslation("Object");
                return;
            }

            writer.WriteKeywordToTranslation(_object);
            return;
        }

        writer.WriteFriendlyName(_type, _settings);
    }
}
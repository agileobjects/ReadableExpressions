namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using Extensions;

internal class GenericConstraintsTranslation : ITranslation
{
    private const string _where = "where ";
    private const string _class = "class";
    private const string _struct = "struct";
    private const string _new = "new()";

    private readonly ITranslation _parameterNameTranslation;
    private readonly bool _isClass;
    private readonly bool _isStruct;
    private readonly bool _isNewable;
    private readonly int _typeConstraintCount;
    private readonly ITranslation[] _typeConstraintTranslations;

    private GenericConstraintsTranslation(
        IGenericParameter genericParameter,
        TranslationSettings settings)
    {
        _parameterNameTranslation = 
            new TypeNameTranslation(genericParameter, settings);

        var translationLength =
            _where.Length +
            _parameterNameTranslation.TranslationLength;

        if (genericParameter.HasClassConstraint)
        {
            translationLength += _class.Length;
            _isClass = true;
        }

        if (genericParameter.HasStructConstraint)
        {
            translationLength += _struct.Length;
            _isStruct = true;
        }

        if (genericParameter.HasNewableConstraint)
        {
            translationLength += _new.Length;
            _isNewable = true;
        }

        var typeConstraints = genericParameter.ConstraintTypes;
        _typeConstraintCount = typeConstraints.Count;

        if (_typeConstraintCount != 0)
        {
            translationLength += _typeConstraintCount * 2;
            _typeConstraintTranslations = new ITranslation[_typeConstraintCount];

            for (var i = 0; i < _typeConstraintCount; ++i)
            {
                var typeNameTranslation = new TypeNameTranslation(typeConstraints[i], settings);
                translationLength += typeNameTranslation.TranslationLength;
                _typeConstraintTranslations[i] = typeNameTranslation;
            }
        }
        else
        {
            _typeConstraintTranslations = Enumerable<ITranslation>.EmptyArray;
        }

        TranslationLength = translationLength;
    }

    #region Factory Method

    public static ITranslation For(
        IGenericParameter genericParameter, 
        TranslationSettings settings)
    {
        return genericParameter.HasConstraints
            ? new GenericConstraintsTranslation(genericParameter, settings)
            : EmptyTranslation.Instance;
    }

    #endregion

    public int TranslationLength { get; }

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation(_where);
        _parameterNameTranslation.WriteTo(writer);
        writer.WriteToTranslation(" : ");

        var constraintWritten = false;

        if (_isClass)
        {
            writer.WriteKeywordToTranslation(_class);
            constraintWritten = true;
        }
        else if (_isStruct)
        {
            writer.WriteKeywordToTranslation(_struct);
            constraintWritten = true;
        }

        if (_typeConstraintCount != 0)
        {
            WriteSeparatorIfNecessary(constraintWritten, writer);

            for (var i = 0; ;)
            {
                _typeConstraintTranslations[i].WriteTo(writer);

                ++i;

                if (i == _typeConstraintCount)
                {
                    break;
                }

                writer.WriteToTranslation(", ");
            }

            constraintWritten = true;
        }

        if (_isNewable)
        {
            WriteSeparatorIfNecessary(constraintWritten, writer);
            writer.WriteKeywordToTranslation("new");
            writer.WriteToTranslation("()");
        }
    }

    private static void WriteSeparatorIfNecessary(
        bool constraintWritten,
        TranslationWriter writer)
    {
        if (constraintWritten)
        {
            writer.WriteToTranslation(", ");
        }
    }
}
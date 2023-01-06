namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Collections.Generic;

/// <summary>
/// An <see cref="ITranslation"/> for a set of generic parameters.
/// </summary>
public class GenericParameterSetDefinitionTranslation : ITranslation
{
    private readonly int _genericParameterCount;
    private readonly ITranslation[] _genericParameterTranslations;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericParameterSetDefinitionTranslation"/>
    /// class.
    /// </summary>
    /// <param name="genericParameters">The <see cref="IGenericParameter"/>s to write to the translation.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public GenericParameterSetDefinitionTranslation(
        IList<IGenericParameter> genericParameters,
        TranslationSettings settings)
    {
        _genericParameterCount = genericParameters.Count;

        if (_genericParameterCount == 0)
        {
            return;
        }

        var translationLength = "<>".Length;

        _genericParameterTranslations = new ITranslation[_genericParameterCount];

        for (var i = 0; ;)
        {
            var argument = genericParameters[i];

            var argumentTranslation = new TypeNameTranslation(argument, settings);

            translationLength += argumentTranslation.TranslationLength;

            _genericParameterTranslations[i] = argumentTranslation;

            ++i;

            if (i == _genericParameterCount)
            {
                break;
            }

            translationLength += ", ".Length;
        }

        TranslationLength = translationLength;
    }

    /// <inheritdoc />
    public int TranslationLength { get; }

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        if (_genericParameterCount == 0)
        {
            return;
        }

        writer.WriteToTranslation('<');

        for (var i = 0; ;)
        {
            _genericParameterTranslations[i].WriteTo(writer);

            ++i;

            if (i == _genericParameterCount)
            {
                break;
            }

            writer.WriteToTranslation(", ");
        }

        writer.WriteToTranslation('>');
    }
}
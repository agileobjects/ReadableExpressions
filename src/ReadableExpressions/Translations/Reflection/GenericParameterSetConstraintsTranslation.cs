namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Collections.Generic;
using static System.Environment;

/// <summary>
/// An <see cref="ITranslation"/> for a set of generic parameter constraints.
/// </summary>
public class GenericParameterSetConstraintsTranslation : ITranslation
{
    private readonly ITranslation[] _constraintTranslations;
    private readonly int _constraintsCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericParameterSetConstraintsTranslation"/>
    /// class.
    /// </summary>
    /// <param name="genericParameters">The <see cref="IGenericParameter"/>s to write to the translation.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public GenericParameterSetConstraintsTranslation(
        IList<IGenericParameter> genericParameters,
        TranslationSettings settings)
    {
        var genericArgumentCount = genericParameters.Count;

        _constraintTranslations = new ITranslation[genericArgumentCount];

        for (var i = 0; ;)
        {
            var parameter = genericParameters[i];

            var constraintsTranslation =
                GenericConstraintsTranslation.For(parameter, settings);

            _constraintTranslations[i] = constraintsTranslation;

            if (constraintsTranslation.TranslationLength > 0)
            {
                ++_constraintsCount;
            }

            ++i;

            if (i == genericArgumentCount)
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    public int TranslationLength
        => _constraintTranslations.TotalTranslationLength(separator: NewLine);

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        if (_constraintsCount == 0)
        {
            return;
        }

        writer.WriteNewLineToTranslation();
        writer.Indent();

        for (var i = 0; ;)
        {
            _constraintTranslations[i].WriteTo(writer);
            ++i;

            if (i == _constraintsCount)
            {
                break;
            }

            writer.WriteNewLineToTranslation();
        }

        writer.Unindent();
    }
}
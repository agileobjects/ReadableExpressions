namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Reflection;
using Extensions;
using static MethodTranslationHelpers;

internal class OperatorDefinitionTranslation : ITranslation
{
    private readonly string _modifiers;
    private readonly ITranslation _returnTypeTranslation;
    private readonly ITranslation _parametersTranslation;

    public OperatorDefinitionTranslation(
        MethodInfo @operator,
        string typeKeyword,
        TranslationSettings settings)
    {
        _modifiers = 
            GetAccessibilityForTranslation(@operator, settings) + 
            "static " + typeKeyword + " operator ";

        _returnTypeTranslation =
            new TypeNameTranslation(@operator.ReturnType, settings);

        _parametersTranslation =
            ParameterSetDefinitionTranslation.For(@operator, settings);
    }

    public int TranslationLength =>
        _modifiers.Length +
        _returnTypeTranslation.TranslationLength +
        _parametersTranslation.TranslationLength;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation(_modifiers);

        _returnTypeTranslation.WriteTo(writer);
        _parametersTranslation.WriteTo(writer);
    }
}
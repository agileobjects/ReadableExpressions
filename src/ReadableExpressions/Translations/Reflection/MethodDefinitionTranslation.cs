namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Reflection;
using Extensions;
using NetStandardPolyfills;

/// <summary>
/// An <see cref="ITranslation"/> for a method signature, including accessibility, scope,
/// generic arguments and constraints, and method parameters.
/// </summary>
public class MethodDefinitionTranslation : ITranslation
{
    private readonly bool _writeModifiers;
    private readonly string _accessibility;
    private readonly string _modifiers;
    private readonly TypeNameTranslation _returnTypeTranslation;
    private readonly TypeNameTranslation _declaringTypeNameTranslation;
    private readonly string _methodName;
    private readonly ITranslation _genericParametersTranslation;
    private readonly ITranslation _genericParameterConstraintsTranslation;
    private readonly ITranslation _parametersTranslation;

    private MethodDefinitionTranslation(
        IMethod method,
        TranslationSettings settings) : 
        this(
            method,
            ParameterSetDefinitionTranslation.For(method, settings),
            includeDeclaringType: true,
            settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDefinitionTranslation"/> class for
    /// the given <paramref name="method"/>.
    /// </summary>
    /// <param name="method">
    /// The <see cref="IMethod"/> for which to create the <see cref="MethodDefinitionTranslation"/>.
    /// </param>
    /// <param name="parametersTranslation">
    /// The <see cref="ITranslation"/> to use to translate the <paramref name="method"/>'s
    /// parameters.
    /// </param>
    /// <param name="includeDeclaringType">
    /// Whether to include the name of the <paramref name="method"/>'s declaring type in the
    /// <see cref="MethodDefinitionTranslation"/>.
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public MethodDefinitionTranslation(
        IMethod method,
        ITranslation parametersTranslation,
        bool includeDeclaringType,
        TranslationSettings settings)
    {
        var translationLength = 0;

        _writeModifiers = !method.IsInterfaceMember();

        if (_writeModifiers)
        {
            _accessibility = method.GetAccessibilityForTranslation();
            _modifiers = method.GetModifiersForTranslation();

            translationLength += _accessibility.Length + _modifiers.Length;
        }

        _returnTypeTranslation =
            new TypeNameTranslation(method.ReturnType, settings);

        _methodName = method.Name;

        translationLength +=
            _returnTypeTranslation.TranslationLength +
            _methodName.Length;

        if (includeDeclaringType && method.DeclaringType != null)
        {
            _declaringTypeNameTranslation =
                new TypeNameTranslation(method.DeclaringType, settings);

            translationLength += _declaringTypeNameTranslation.TranslationLength;
        }

        if (method.IsGenericMethod)
        {
            var genericArguments = method.GetGenericArguments();

            _genericParametersTranslation =
                new GenericParameterSetDefinitionTranslation(genericArguments, settings);

            _genericParameterConstraintsTranslation =
                new GenericParameterSetConstraintsTranslation(genericArguments, settings);

            translationLength +=
                _genericParametersTranslation.TranslationLength +
                _genericParameterConstraintsTranslation.TranslationLength;
        }

        _parametersTranslation = parametersTranslation;

        TranslationLength = translationLength + _parametersTranslation.TranslationLength;
    }

    /// <summary>
    /// Creates an <see cref="ITranslation"/> for the given <paramref name="method"/>, handling
    /// properties and operators as well as regular methods. Includes the declaring type name in
    /// the translation.
    /// </summary>
    /// <param name="method">The MethodInfo for which to create the <see cref="ITranslation"/>.</param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    /// <returns>An <see cref="ITranslation"/> for the given <paramref name="method"/>.</returns>
    public static ITranslation For(MethodInfo method, TranslationSettings settings)
    {
        if (method.IsAccessor(out var property))
        {
            return new PropertyDefinitionTranslation(property, method, settings);
        }

        if (method.IsImplicitOperator())
        {
            return new OperatorDefinitionTranslation(method, "implicit", settings);
        }

        if (method.IsExplicitOperator())
        {
            return new OperatorDefinitionTranslation(method, "explicit", settings);
        }

        return new MethodDefinitionTranslation(
            new ClrMethodWrapper(method, settings),
            settings);
    }

    /// <inheritdoc />
    public int TranslationLength { get; }

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        if (_writeModifiers)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);
        }

        _returnTypeTranslation.WriteTo(writer);
        writer.WriteSpaceToTranslation();

        if (_declaringTypeNameTranslation != null)
        {
            _declaringTypeNameTranslation.WriteTo(writer);
            writer.WriteDotToTranslation();
        }

        writer.WriteToTranslation(_methodName);

        _genericParametersTranslation?.WriteTo(writer);
        _parametersTranslation.WriteTo(writer);
        _genericParameterConstraintsTranslation?.WriteTo(writer);
    }
}
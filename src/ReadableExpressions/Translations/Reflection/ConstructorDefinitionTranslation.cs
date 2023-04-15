namespace AgileObjects.ReadableExpressions.Translations.Reflection;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions;

/// <summary>
/// An <see cref="ITranslation"/> for a method signature, including accessibility, scope,
/// and constructor parameters.
/// </summary>
public class ConstructorDefinitionTranslation : INodeTranslation
{
    private readonly string _modifiers;
    private readonly INodeTranslation _typeNameTranslation;
    private readonly ITranslation _parametersTranslation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorDefinitionTranslation"/> class
    /// for the given <paramref name="ctorInfo"/>.
    /// </summary>
    /// <param name="ctorInfo">
    /// The ConstructorInfo for which to create the
    /// <see cref="ConstructorDefinitionTranslation"/>.
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public ConstructorDefinitionTranslation(
        ConstructorInfo ctorInfo,
        TranslationSettings settings)
        : this(new ClrCtorInfoWrapper(ctorInfo), settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorDefinitionTranslation"/> class
    /// for the given <paramref name="ctor"/>.
    /// </summary>
    /// <param name="ctor">
    /// The <see cref="IConstructor"/> describing the ConstructorInfo for which to create the
    /// <see cref="ConstructorDefinitionTranslation"/>.
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public ConstructorDefinitionTranslation(
        IConstructor ctor,
        TranslationSettings settings)
        : this(
            ctor,
            ParameterSetDefinitionTranslation.For(ctor, settings),
            settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorDefinitionTranslation"/> class
    /// for the given <paramref name="ctor"/> and with the given
    /// <paramref name="parametersTranslation"/>.
    /// </summary>
    /// <param name="ctor">
    /// The <see cref="IConstructor"/> describing the ConstructorInfo for which to create the
    /// <see cref="ConstructorDefinitionTranslation"/>.
    /// </param>
    /// <param name="parametersTranslation">
    /// The <see cref="ITranslation"/> to use to translate the <paramref name="ctor"/>'s
    /// parameters.
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    public ConstructorDefinitionTranslation(
        IConstructor ctor,
        ITranslation parametersTranslation,
        TranslationSettings settings)
    {
        _modifiers = ctor.IsStatic
            ? "static "
            : ctor.GetAccessibilityForTranslation();

        _typeNameTranslation = new TypeNameTranslation(ctor.DeclaringType, settings);
        _parametersTranslation = parametersTranslation;
    }

    /// <inheritdoc />
    public ExpressionType NodeType => ExpressionType.New;

    /// <inheritdoc />
    public int TranslationLength =>
        _typeNameTranslation.TranslationLength +
        _parametersTranslation.TranslationLength;

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation(_modifiers);

        _typeNameTranslation.WriteTo(writer);
        _parametersTranslation.WriteTo(writer);
    }
}
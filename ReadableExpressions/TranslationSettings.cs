namespace AgileObjects.ReadableExpressions;

using System;
using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Translations;
using Translations.Formatting;

/// <summary>
/// Provides configuration options to control aspects of source-code string generation.
/// </summary>
public class TranslationSettings : ITranslationSettings
{
    internal static readonly TranslationSettings Default = new();

    private bool _commentQuotedLambdas;
    private Dictionary<ExpressionType, SourceCodeTranslationFactory> _customTranslationFactories;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationSettings"/> class.
    /// </summary>
    public TranslationSettings()
    {
        UseImplicitTypeNames = true;
        UseImplicitGenericParameters = true;
        HideImplicitlyTypedArrayTypes = true;
        Indent = "    ";
        Formatter = NullTranslationFormatter.Instance;
    }

    /// <summary>
    /// Configure the default settings to use in Expression translation. By default:
    /// <br />
    /// - 'var' is used instead of Type names where possible<br />
    /// - Generic Type argument names are hidden where possible<br />
    /// - Array element Type names are hidden where possible<br />
    /// - Code is indented using four spaces<br />
    /// </summary>
    /// <param name="configuration">The default configuration to use for Expression translation.</param>
    public static void ConfigureDefaults(Action<ITranslationSettings> configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration.Invoke(Default);
    }

    ITranslationSettings ITranslationSettings.UseFullyQualifiedTypeNames
    {
        get
        {
            FullyQualifyTypeNames = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether type names should be fully-qualified with their namespace.
    /// </summary>
    public bool FullyQualifyTypeNames { get; private set; }

    ITranslationSettings ITranslationSettings.UseExplicitTypeNames
    {
        get
        {
            UseImplicitTypeNames = false;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether 'var' should be used instead of full type names local
    /// and inline-declared output parameter variables.
    /// </summary>
    public bool UseImplicitTypeNames { get; private set; }

    ITranslationSettings ITranslationSettings.UseExplicitGenericParameters
    {
        get
        {
            UseImplicitGenericParameters = false;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether implicitly-typed generic parameter arguments should be
    /// excluded from method calls when possible.
    /// </summary>
    public bool UseImplicitGenericParameters { get; private set; }

    ITranslationSettings ITranslationSettings.DeclareOutputParametersInline
    {
        get
        {
            DeclareOutParamsInline = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether output parameter varables should be declared inline with
    /// the method call where they are first used.
    /// </summary>
    public bool DeclareOutParamsInline { get; private set; }

    ITranslationSettings ITranslationSettings.DiscardUnusedParameters
    {
        get
        {
            DiscardUnusedParams = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether discards (_) should be used in place of unused
    /// parameters.
    /// </summary>
    public bool DiscardUnusedParams { get; private set; }

    ITranslationSettings ITranslationSettings.ShowImplicitArrayTypes
    {
        get
        {
            HideImplicitlyTypedArrayTypes = false;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the element type names of implicitly-typed arrays should
    /// be hidden.
    /// </summary>
    public bool HideImplicitlyTypedArrayTypes { get; private set; }

    ITranslationSettings ITranslationSettings.ShowLambdaParameterTypes
    {
        get
        {
            ShowLambdaParamTypes = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the names of Lambda Expression parameter types should
    /// be shown.
    /// </summary>
    public bool ShowLambdaParamTypes { get; private set; }

    ITranslationSettings ITranslationSettings.ShowQuotedLambdaComments
    {
        get
        {
            _commentQuotedLambdas = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether Quoted Lambda Expressions not should be annotated with
    /// a comment indicating they have been Quoted.
    /// </summary>
    public bool DoNotCommentQuotedLambdas => !_commentQuotedLambdas;

    ITranslationSettings ITranslationSettings.ShowCapturedValues
    {
        get
        {
            ShowCapturedValues = true;
            return this;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the value of any captured variables or members should be
    /// shown instead of the captured variable or member name.
    /// </summary>
    public bool ShowCapturedValues { get; private set; }

    ITranslationSettings ITranslationSettings.AddTranslatorFor(
        ExpressionType nodeType,
        SourceCodeTranslationFactory translationFactory)
    {
        _customTranslationFactories ??= new();
        _customTranslationFactories[nodeType] = translationFactory;
        return this;
    }

    internal bool TryGetCustomTranslationFor(
        Expression expression,
        ExpressionTranslation context,
        out INodeTranslation customTranslation)
    {
        if (_customTranslationFactories == null)
        {
            customTranslation = null;
            return false;
        }

        var customFactoryExists = _customTranslationFactories
            .TryGetValue(expression.NodeType, out var configuredFactory);

        if (customFactoryExists)
        {
            customTranslation = new StringFactoryTranslation(
                expression,
                configuredFactory,
                context);

            return true;
        }

        customTranslation = null;
        return false;
    }

    ITranslationSettings ITranslationSettings.NameAnonymousTypesUsing(Func<Type, string> nameFactory)
    {
        AnonymousTypeNameFactory = nameFactory;
        return this;
    }

    /// <summary>
    /// Gets a factory to use to name anonymous types instead of the default method.
    /// </summary>
    public Func<Type, string> AnonymousTypeNameFactory { get; private set; }

    ITranslationSettings ITranslationSettings.IndentUsing(string indent)
    {
        Indent = indent;
        return this;
    }

    /// <summary>
    /// Gets the whitespace string with which to indent multi-line Expression translations.
    /// </summary>
    public string Indent { get; private set; }

    ITranslationSettings ITranslationSettings.FormatUsing(ITranslationFormatter formatter)
    {
        Formatter = formatter;
        return this;
    }

    /// <summary>
    /// Gats the <see cref="ITranslationFormatter"/> with which to format the translation.
    /// </summary>
    public ITranslationFormatter Formatter { get; private set; }
}
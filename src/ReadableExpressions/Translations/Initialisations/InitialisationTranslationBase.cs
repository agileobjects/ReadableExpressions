﻿namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

using System;
using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal abstract class InitialisationTranslationBase<TInitializer> : INodeTranslation
{
    private readonly INodeTranslation _newingTranslation;
    private readonly IInitializerSetTranslation _initializerTranslations;

    protected InitialisationTranslationBase(
        ExpressionType initType,
        NewExpression newing,
        IInitializerSetTranslation initializerTranslations,
        ITranslationContext context) :
        this(
            initType,
            NewingTranslation.For(
                newing,
                context,
                omitParenthesesIfParameterless: initializerTranslations.Count != 0),
            initializerTranslations)
    {
    }

    protected InitialisationTranslationBase(
        ExpressionType initType,
        INodeTranslation newingTranslation,
        IInitializerSetTranslation initializerTranslations)
    {
        initializerTranslations.Parent = this;

        NodeType = initType;
        _newingTranslation = newingTranslation;
        _initializerTranslations = initializerTranslations;
    }

    protected static bool InitHasNoInitializers(
        NewExpression newing,
        ICollection<TInitializer> initializers,
        ITranslationContext context,
        out INodeTranslation newingTranslation)
    {
        var hasInitializers = initializers.Count != 0;

        newingTranslation = NewingTranslation.For(
            newing,
            context,
            omitParenthesesIfParameterless: hasInitializers);

        return hasInitializers == false;
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength =>
        _newingTranslation.TranslationLength +
        _initializerTranslations.TranslationLength;

    public void WriteTo(TranslationWriter writer)
    {
        _newingTranslation.WriteTo(writer);
        _initializerTranslations.WriteTo(writer);
    }
}
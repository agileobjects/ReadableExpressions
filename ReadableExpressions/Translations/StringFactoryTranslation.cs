﻿namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions.Translations;
#endif
using Extensions;

internal class StringFactoryTranslation : INodeTranslation
{
    private readonly ITranslationContext _context;
    private readonly SourceCodeTranslationFactory _translationFactory;
    private readonly Expression _expression;
    private string _translation;

    public StringFactoryTranslation(
        Expression expression,
        SourceCodeTranslationFactory translationFactory,
        ITranslationContext context)
    {
        _context = context;
        _translationFactory = translationFactory;
        _expression = expression;
    }

    public ExpressionType NodeType => _expression.NodeType;

    public int TranslationLength => GetTranslation()?.Length ?? 0;

    public void WriteTo(TranslationWriter writer)
        => writer.WriteToTranslation(GetTranslation());

    private string GetTranslation()
    {
        return _translation ??= _translationFactory
            .Invoke(_expression, expression => _context
                .GetTranslationFor(expression)?
                .WriteUsing(_context.Settings));
    }
}
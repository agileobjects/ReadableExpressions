﻿namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
using NetStandardPolyfills;

internal class IndexAccessTranslation : INodeTranslation
{
    private readonly INodeTranslation _subject;
    private readonly ParameterSetTranslation _parameters;
    private readonly ITranslationContext _context;

    public IndexAccessTranslation(
        MethodCallExpression indexAccessCall,
        ParameterSetTranslation parameters,
        ITranslationContext context) :
        this(
            context.GetTranslationFor(indexAccessCall.Object),
            parameters,
            context)
    {
        NodeType = ExpressionType.Call;
    }

    private IndexAccessTranslation(
        IndexExpression indexAccess,
        ITranslationContext context) :
        this(indexAccess.Object, indexAccess.Arguments, context)
    {
        NodeType = ExpressionType.Index;
    }

    public IndexAccessTranslation(
        BinaryExpression arrayIndexAccess,
        ITranslationContext context) :
        this(
            arrayIndexAccess.Left,
           [arrayIndexAccess.Right],
            context)
    {
        NodeType = ExpressionType.ArrayIndex;
    }

    private IndexAccessTranslation(
        Expression subject,
        ICollection<Expression> arguments,
        ITranslationContext context) :
        this(
            context.GetTranslationFor(subject),
            ParameterSetTranslation.For(arguments, context),
            context)
    {
    }

    private IndexAccessTranslation(
        INodeTranslation subject,
        ParameterSetTranslation parameters,
        ITranslationContext context)
    {
        _subject = subject;
        _parameters = parameters;
        _context = context;
    }

    #region Factory Methods

    public static INodeTranslation For(
        IndexExpression indexAccess,
        ITranslationContext context)
    {
        var indexer = indexAccess.Indexer;

        if (indexer == null)
        {
            return new IndexAccessTranslation(indexAccess, context);
        }

        var indexAccessor = indexer.GetGetter() ?? indexer.GetSetter();

        if (indexAccessor.IsHideBySig)
        {
            return new IndexAccessTranslation(indexAccess, context);
        }

        var indexCall = Expression.Call(
            indexAccess.Object,
            indexAccessor,
            indexAccess.Arguments);

        return MethodCallTranslation.For(indexCall, context);
    }

    #endregion

    public ExpressionType NodeType { get; }

    public int TranslationLength => 
        _subject.TranslationLength + _parameters.TranslationLength + "[]".Length;

    public void WriteTo(TranslationWriter writer)
    {
        _subject.WriteInParenthesesIfRequired(writer, _context);
        writer.WriteToTranslation('[');
        _parameters.WithoutParentheses().WriteTo(writer);
        writer.WriteToTranslation(']');
    }
}
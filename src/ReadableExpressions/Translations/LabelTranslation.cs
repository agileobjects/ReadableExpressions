namespace AgileObjects.ReadableExpressions.Translations;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class LabelTranslation :
    INodeTranslation,
    IPotentialSelfTerminatingTranslation,
    IPotentialGotoTranslation
{
    private readonly string _labelName;
    private readonly bool _labelIsNamed;
    private readonly bool _labelHasNoValue;
    private readonly ITranslation _labelValueTranslation;

    private LabelTranslation(
        LabelExpression label,
        string labelName,
        ITranslationContext context)
    {
        _labelName = labelName;
        _labelHasNoValue = label.DefaultValue == null;

        if (_labelName != null)
        {
            _labelIsNamed = true;

        }

        if (HasGoto)
        {
            _labelValueTranslation = context
                .GetCodeBlockTranslationFor(label.DefaultValue);
        }
    }

    #region Factory Methods

    public static INodeTranslation For(LabelExpression label, ITranslationContext context)
    {
        var labelName = GetLabelNamePart(label, context);

        if (labelName == null && label.DefaultValue == null)
        {
            return new EmptyLabelTranslation();
        }

        return new LabelTranslation(label, labelName, context);
    }

    #endregion

    private static string GetLabelNamePart(
        LabelExpression label,
        ITranslationContext context)
    {
        if (context.Analysis.IsReferencedByGoto(label.Target))
        {
            return label.Target.Name.IsNullOrWhiteSpace() ? null : label.Target.Name;
        }

        return null;
    }

    public ExpressionType NodeType => ExpressionType.Label;

    public int TranslationLength
    {
        get
        {
            var translationLength = 0;

            if (_labelName != null)
            {
                translationLength += _labelName.Length + 1 + Environment.NewLine.Length;
            }

            if (HasGoto)
            {
                translationLength += _labelValueTranslation.TranslationLength;
            }

            return translationLength;
        }
    }

    public bool IsTerminated => true;

    public bool HasGoto => !_labelHasNoValue;

    public void WriteTo(TranslationWriter writer)
    {
        if (_labelIsNamed)
        {
            writer.WriteToTranslation(_labelName);
            writer.WriteToTranslation(':');
        }

        if (_labelHasNoValue)
        {
            return;
        }

        writer.WriteReturnToTranslation();
        _labelValueTranslation.WriteTo(writer);
        writer.WriteSemiColonToTranslation();
    }

    private class EmptyLabelTranslation : EmptyTranslation, INodeTranslation
    {
        public ExpressionType NodeType => ExpressionType.Label;
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class LabelTranslation :
        ITranslation,
        IPotentialSelfTerminatingTranslatable,
        IPotentialGotoTranslatable
    {
        private readonly string _labelName;
        private readonly bool _labelIsNamed;
        private readonly bool _labelHasNoValue;
        private readonly ITranslatable _labelValueTranslation;

        private LabelTranslation(
            LabelExpression label,
            string labelName,
            ITranslationContext context)
        {
            Type = label.Type;
            _labelName = labelName;
            _labelHasNoValue = label.DefaultValue == null;

            if (_labelName != null)
            {
                _labelIsNamed = true;
                TranslationSize = _labelName.Length + 1 + Environment.NewLine.Length;
            }

            if (_labelHasNoValue)
            {
                return;
            }

            _labelValueTranslation = context.GetCodeBlockTranslationFor(label.DefaultValue);
            TranslationSize += _labelValueTranslation.TranslationSize;
            FormattingSize = _labelValueTranslation.FormattingSize;
        }

        #region Factory Methods

        public static ITranslation For(LabelExpression label, ITranslationContext context)
        {
            var labelName = GetLabelNamePart(label, context);

            if (labelName == null && label.DefaultValue == null)
            {
                return new EmptyLabelTranslation(label);
            }

            return new LabelTranslation(label, labelName, context);
        }

        #endregion

        private static string GetLabelNamePart(LabelExpression label, ITranslationContext context)
        {
            if (context.Analysis.IsReferencedByGoto(label.Target))
            {
                return label.Target.Name.IsNullOrWhiteSpace() ? null : label.Target.Name;
            }

            return null;
        }

        public ExpressionType NodeType => ExpressionType.Label;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsTerminated => true;
        
        public bool HasGoto => !_labelHasNoValue;

        public int GetIndentSize() => _labelValueTranslation?.GetIndentSize() ?? 0;

        public int GetLineCount() => _labelValueTranslation?.GetLineCount() ?? 1;

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
            writer.WriteToTranslation(';');
        }

        private class EmptyLabelTranslation : EmptyTranslatable, ITranslation
        {
            public EmptyLabelTranslation(Expression label)
            {
                Type = label.Type;
            }

            public ExpressionType NodeType => ExpressionType.Label;

            public Type Type { get; }
        }
    }
}
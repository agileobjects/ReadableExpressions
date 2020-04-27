namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class LabelTranslation :
        ITranslation,
        IPotentialSelfTerminatingTranslatable,
        IPotentialEmptyTranslatable,
        IPotentialGotoTranslatable
    {
        private readonly string _labelName;
        private readonly bool _labelIsNamed;
        private readonly bool _labelHasNoValue;
        private readonly CodeBlockTranslation _labelValueTranslation;

        public LabelTranslation(LabelExpression label, ITranslationContext context)
        {
            Type = label.Type;
            _labelName = GetLabelNamePart(label, context);
            _labelIsNamed = _labelName != null;
            _labelHasNoValue = label.DefaultValue == null;

            if (_labelIsNamed)
            {
                // ReSharper disable once PossibleNullReferenceException
                EstimatedSize = _labelName.Length + 1 + Environment.NewLine.Length;
            }
            else if (_labelHasNoValue)
            {
                IsEmpty = true;
                return;
            }

            IsTerminated = true;

            if (_labelHasNoValue)
            {
                return;
            }

            _labelValueTranslation = context.GetCodeBlockTranslationFor(label.DefaultValue);
            EstimatedSize += _labelValueTranslation.EstimatedSize;
        }

        private static string GetLabelNamePart(LabelExpression label, ITranslationContext context)
        {
            if (context.IsReferencedByGoto(label.Target))
            {
                return label.Target.Name.IsNullOrWhiteSpace() ? null : label.Target.Name;
            }

            return null;
        }

        public ExpressionType NodeType => ExpressionType.Label;
        
        public Type Type { get; }

        public int EstimatedSize { get; }

        public bool IsTerminated { get; }

        public bool IsEmpty { get; }

        public bool HasGoto => !_labelHasNoValue;

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_labelIsNamed)
            {
                buffer.WriteToTranslation(_labelName);
                buffer.WriteToTranslation(':');
            }

            if (_labelHasNoValue)
            {
                return;
            }

            buffer.WriteReturnToTranslation();
            _labelValueTranslation.WriteTo(buffer);
            buffer.WriteToTranslation(';');
        }
    }
}
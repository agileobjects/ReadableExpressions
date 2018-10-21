namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class LabelTranslation : ITranslation
    {
        private readonly string _labelName;

        public LabelTranslation(LabelExpression label, ITranslationContext context)
        {
            _labelName = GetLabelNamePart(label, context);

            if (_labelName == null)
            {
                return;
            }

            EstimatedSize = _labelName.Length + 1 + Environment.NewLine.Length;
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

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_labelName == null)
            {
                return;
            }

            context.WriteToTranslation(_labelName);
            context.WriteToTranslation(':');
            context.WriteNewLineToTranslation();
        }
    }
}
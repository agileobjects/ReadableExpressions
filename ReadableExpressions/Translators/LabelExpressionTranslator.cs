namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LabelExpressionTranslator : ExpressionTranslatorBase
    {
        public LabelExpressionTranslator()
            : base(ExpressionType.Label)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var label = (LabelExpression)expression;

            var labelNamePart = GetLabelNamePart(label, context);

            if (label.DefaultValue == null)
            {
                return labelNamePart;
            }
            var labelValuePart = $"{Environment.NewLine}return {context.TranslateAsCodeBlock(label.DefaultValue)};";

            return labelNamePart + labelValuePart;
        }

        private static string GetLabelNamePart(LabelExpression label, TranslationContext context)
        {
            if (context.IsReferencedByGoto(label.Target))
            {
                return string.IsNullOrWhiteSpace(label.Target.Name)
                   ? null
                   : Environment.NewLine + label.Target.Name.Unindented() + ":";
            }

            return null;
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LabelExpressionTranslator : ExpressionTranslatorBase
    {
        public LabelExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Label)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var label = (LabelExpression)expression;

            var labelNamePart = GetLabelNamePart(label, context);

            var labelValuePart = (label.DefaultValue != null)
                ? $"{Environment.NewLine}return {GetTranslation(label.DefaultValue, context)};"
                : null;

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
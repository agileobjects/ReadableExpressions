namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LabelExpressionTranslator : ExpressionTranslatorBase
    {
        public LabelExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Label)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var label = (LabelExpression)expression;

            var labelNamePart = string.IsNullOrWhiteSpace(label.Target.Name)
                ? null
                : Environment.NewLine + label.Target.Name.Unindented() + ":";

            var labelValuePart = (label.DefaultValue != null)
                ? $"{Environment.NewLine}return {GetTranslation(label.DefaultValue, context)};"
                : null;

            return $"{labelNamePart}{labelValuePart}";
        }
    }
}
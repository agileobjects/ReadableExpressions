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

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var label = (LabelExpression)expression;

            var labelNamePart = string.IsNullOrWhiteSpace(label.Target.Name)
                ? null
                : $"{Environment.NewLine}{label.Target.Name.Unindented()}:";

            var labelValuePart = (label.DefaultValue != null)
                ? $"{Environment.NewLine}return {translatorRegistry.Translate(label.DefaultValue)};"
                : null;

            return $"{labelNamePart}{labelValuePart}";
        }
    }
}
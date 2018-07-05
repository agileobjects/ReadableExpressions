namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using Extensions;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using LabelExpression = Microsoft.Scripting.Ast.LabelExpression;
#endif

    internal struct LabelExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Label; }
        }

        public string Translate(Expression expression, TranslationContext context)
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
                return label.Target.Name.IsNullOrWhiteSpace()
                   ? null
                   : Environment.NewLine + label.Target.Name.Unindented() + ":";
            }

            return null;
        }
    }
}
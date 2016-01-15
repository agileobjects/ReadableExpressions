namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ConstantExpressionTranslator : ExpressionTranslatorBase
    {
        internal ConstantExpressionTranslator()
            : base(ExpressionType.Constant)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            return (expression as CommentExpression)?.Comment
                ?? TranslateConstant((ConstantExpression)expression);
        }

        private string TranslateConstant(ConstantExpression constant)
        {
            if (constant.Type == typeof(string))
            {
                return "\"" + constant.Value + "\"";
            }

            return constant.Value.ToString();
        }
    }
}
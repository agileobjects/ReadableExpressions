namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

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

        private static string TranslateConstant(ConstantExpression constant)
        {
            if (constant.Type == typeof(string))
            {
                return "\"" + constant.Value + "\"";
            }

            if (constant.Type.IsEnum)
            {
                return constant.Type.GetFriendlyName() + "." + constant.Value;
            }

            return constant.Value.ToString();
        }
    }
}
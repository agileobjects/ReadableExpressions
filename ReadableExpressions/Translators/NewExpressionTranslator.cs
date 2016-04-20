namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.New)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var newExpression = (NewExpression)expression;
            var parameters = GetTranslatedParameters(newExpression.Arguments, context).WithParentheses();

            return "new " + newExpression.Type.GetFriendlyName() + parameters;
        }
    }
}
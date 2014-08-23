namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator()
            : base(ExpressionType.New)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var newExpression = (NewExpression)expression;

            return "new " + newExpression.Type.GetFriendlyName() + "()";
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class MemberAccessExpressionTranslator : ExpressionTranslatorBase
    {
        internal MemberAccessExpressionTranslator()
            : base(ExpressionType.MemberAccess)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var memberExpression = (MemberExpression)expression;
            var memberSubject = translatorRegistry.Translate(memberExpression.Expression);

            return memberSubject + "." + memberExpression.Member.Name;
        }
    }
}
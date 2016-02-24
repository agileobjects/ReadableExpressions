namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class MemberAccessExpressionTranslator : ExpressionTranslatorBase
    {
        internal MemberAccessExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.MemberAccess)
        {
        }

        public override string Translate(Expression expression)
        {
            var memberExpression = (MemberExpression)expression;
            var memberSubject = GetMemberSubject(memberExpression);

            return memberSubject + "." + memberExpression.Member.Name;
        }

        private string GetMemberSubject(MemberExpression memberExpression)
        {
            return (memberExpression.Expression != null)
                ? Registry.Translate(memberExpression.Expression)
                : memberExpression.Type.Name;
        }
    }
}
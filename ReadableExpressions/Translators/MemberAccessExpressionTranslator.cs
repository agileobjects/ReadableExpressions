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
            var subject = GetSubject(memberExpression);

            return GetMemberAccess(subject, memberExpression.Member.Name);
        }

        private string GetSubject(MemberExpression memberExpression)
        {
            return (memberExpression.Expression != null)
                ? Registry.Translate(memberExpression.Expression)
                : memberExpression.Type.Name;
        }

        internal string GetMemberAccess(string subject, string memberName)
        {
            return subject + "." + memberName;
        }
    }
}
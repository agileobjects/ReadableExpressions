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
                ? GetInstanceMemberSubject(memberExpression)
                : memberExpression.Type.Name;
        }

        private string GetInstanceMemberSubject(MemberExpression memberExpression)
        {
            return SubjectIsCapturedInstance(memberExpression)
                ? null
                : Registry.Translate(memberExpression.Expression);
        }

        private static bool SubjectIsCapturedInstance(MemberExpression memberExpression)
        {
            if (memberExpression.Expression.NodeType != ExpressionType.Constant)
            {
                return false;
            }

            var subjectType = ((ConstantExpression)memberExpression.Expression).Type;

            return subjectType == memberExpression.Member.DeclaringType;
        }

        internal string GetMemberAccess(string subject, string memberName)
        {
            if (subject != null)
            {
                subject += ".";
            }

            return subject + memberName;
        }
    }
}
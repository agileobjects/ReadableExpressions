namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Extensions;

    internal class MemberAccessExpressionTranslator : ExpressionTranslatorBase
    {
        internal MemberAccessExpressionTranslator()
            : base(ExpressionType.MemberAccess)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var memberExpression = (MemberExpression)expression;
            var subject = GetSubject(memberExpression, context);

            return GetMemberAccess(subject, memberExpression.Member.Name);
        }

        private static string GetSubject(MemberExpression memberExpression, TranslationContext context)
        {
            return (memberExpression.Expression != null)
                ? GetInstanceMemberSubject(memberExpression, context)
                : memberExpression.Member.DeclaringType.GetFriendlyName();
        }

        private static string GetInstanceMemberSubject(MemberExpression memberExpression, TranslationContext context)
        {
            return SubjectIsCapturedInstance(memberExpression)
                ? null
                : context.Translate(memberExpression.Expression);
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
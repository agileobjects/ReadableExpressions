namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class MemberAccessExpressionTranslator : ExpressionTranslatorBase
    {
        internal MemberAccessExpressionTranslator(Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.MemberAccess)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var memberExpression = (MemberExpression)expression;
            var subject = GetSubject(memberExpression, context);

            return GetMemberAccess(subject, memberExpression.Member.Name);
        }

        private string GetSubject(MemberExpression memberExpression, TranslationContext context)
        {
            return (memberExpression.Expression != null)
                ? GetInstanceMemberSubject(memberExpression, context)
                : memberExpression.Type.Name;
        }

        private string GetInstanceMemberSubject(MemberExpression memberExpression, TranslationContext context)
        {
            return SubjectIsCapturedInstance(memberExpression)
                ? null
                : GetTranslation(memberExpression.Expression, context);
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
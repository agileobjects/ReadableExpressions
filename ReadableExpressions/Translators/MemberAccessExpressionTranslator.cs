namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using ConstantExpression = Microsoft.Scripting.Ast.ConstantExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using MemberExpression = Microsoft.Scripting.Ast.MemberExpression;
#endif
    using Extensions;

    internal struct MemberAccessExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.MemberAccess; }
        }

        public string Translate(Expression expression, TranslationContext context)
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

        internal static string GetMemberAccess(string subject, string memberName)
        {
            if (subject != null)
            {
                subject += ".";
            }

            return subject + memberName;
        }
    }
}
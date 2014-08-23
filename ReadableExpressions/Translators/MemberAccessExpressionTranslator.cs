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
            var memberSubject = GetMemberSubject(memberExpression, translatorRegistry);

            return memberSubject + "." + memberExpression.Member.Name;
        }

        private static string GetMemberSubject(
            MemberExpression memberExpression,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            return (memberExpression.Expression != null)
                ? translatorRegistry.Translate(memberExpression.Expression)
                : memberExpression.Type.Name;
        }
    }
}
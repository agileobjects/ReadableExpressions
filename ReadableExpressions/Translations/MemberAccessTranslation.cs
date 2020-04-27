namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class MemberAccessTranslation : ITranslation
    {
        private readonly string _memberName;
        private readonly ITranslation _subject;

        public MemberAccessTranslation(MemberExpression memberAccess, ITranslationContext context)
            : this(memberAccess.Member.Name)
        {
            Type = memberAccess.Type;
            _subject = GetSubjectOrNull(memberAccess, context);
            EstimatedSize = GetEstimatedSize();
        }

        private static ITranslation GetSubjectOrNull(MemberExpression memberAccess, ITranslationContext context)
            => GetSubjectOrNull(memberAccess.Expression, memberAccess.Member, context);

        protected static ITranslation GetSubjectOrNull(
            Expression subject,
            MemberInfo member,
            ITranslationContext context)
        {
            if (subject == null)
            {
                return context.GetTranslationFor(member.DeclaringType);
            }

            if (SubjectIsCapturedInstance(subject, member))
            {
                return null;
            }

            return context.GetTranslationFor(subject);
        }

        private static bool SubjectIsCapturedInstance(Expression subject, MemberInfo member)
        {
            if (subject.NodeType != ExpressionType.Constant)
            {
                return false;
            }

            var subjectType = ((ConstantExpression)subject).Type;

            return subjectType == member.DeclaringType;
        }

        public MemberAccessTranslation(ITranslation subject, string memberName, Type memberType)
            : this(memberName)
        {
            Type = memberType;
            _subject = subject;
            EstimatedSize = GetEstimatedSize();
        }

        private MemberAccessTranslation(string memberName)
        {
            _memberName = memberName;
        }

        private int GetEstimatedSize()
        {
            return (_subject != null)
                  ? _subject.EstimatedSize + ".".Length + _memberName.Length
                  : _memberName.Length;
        }

        public ExpressionType NodeType => ExpressionType.MemberAccess;

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_subject != null)
            {
                _subject.WriteInParenthesesIfRequired(buffer);
                buffer.WriteDotToTranslation();
            }

            buffer.WriteToTranslation(_memberName);
        }
    }
}

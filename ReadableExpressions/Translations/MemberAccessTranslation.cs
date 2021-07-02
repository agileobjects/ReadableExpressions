namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;
    using Formatting;

    internal class MemberAccessTranslation : ITranslation
    {
        private readonly ITranslationContext _context;
        private readonly string _memberName;
        private readonly ITranslation _subject;

        public MemberAccessTranslation(MemberExpression memberAccess, ITranslationContext context)
            : this(memberAccess.Member.Name, context)
        {
            Type = memberAccess.Type;
            _subject = GetSubjectOrNull(memberAccess, context);
            TranslationSize = GetTranslationSize();
            FormattingSize = GetFormattingSize();
        }

        #region Setup

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

        #endregion 

        public MemberAccessTranslation(
            ITranslation subject,
            string memberName,
            Type memberType,
            ITranslationContext context)
            : this(memberName, context)
        {
            Type = memberType;
            _subject = subject;
            TranslationSize = GetTranslationSize();
            FormattingSize = GetFormattingSize();
        }

        private MemberAccessTranslation(string memberName, ITranslationContext context)
        {
            _context = context;
            _memberName = memberName;
        }

        private int GetTranslationSize()
        {
            return (_subject != null)
                ? _subject.TranslationSize + ".".Length + _memberName.Length
                : _memberName.Length;
        }

        private int GetFormattingSize()
            => _subject?.FormattingSize ?? +_context.GetFormattingSize(TokenType.MethodName);

        public ExpressionType NodeType => ExpressionType.MemberAccess;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _subject?.GetIndentSize() ?? 0;

        public int GetLineCount() => _subject?.GetLineCount() ?? 1;

        public void WriteTo(TranslationWriter writer)
        {
            if (_subject != null)
            {
                _subject.WriteInParenthesesIfRequired(writer, _context);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_memberName);
        }
    }
}

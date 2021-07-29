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
        private readonly ITranslation _subjectTranslation;
        private readonly string _memberName;

        public MemberAccessTranslation(
            ITranslation subjectTranslation,
            Type memberType,
            string memberName,
            ITranslationContext context)
        {
            _subjectTranslation = subjectTranslation;
            Type = memberType;
            _memberName = memberName;
            _context = context;

            TranslationSize = GetTranslationSize();
            FormattingSize = GetFormattingSize();
        }

        #region Factory Methods

        public static ITranslation For(MemberExpression memberAccess, ITranslationContext context)
        {
            bool translateSubject;

            if (memberAccess.IsCapturedValue(out var capturedValue, out var isStatic))
            {
                if (context.Settings.ShowCapturedValues)
                {
                    return context.GetTranslationFor(
                        Expression.Constant(capturedValue, memberAccess.Type));
                }

                translateSubject = isStatic;
            }
            else
            {
                translateSubject = true;
            }

            var subjectTranslation = translateSubject
                ? TranslateSubject(memberAccess, context)
                : null;

            return new MemberAccessTranslation(
                subjectTranslation,
                memberAccess.Type,
                memberAccess.Member.Name,
                context);
        }

        private static ITranslation TranslateSubject(
            MemberExpression memberAccess, 
            ITranslationContext context)
        {
            return GetSubjectTranslationOrNull(
                memberAccess.Expression,
                memberAccess.Member,
                context);
        }

        protected static ITranslation GetSubjectTranslationOrNull(
            Expression subject,
            MemberInfo member,
            ITranslationContext context)
        {
            return subject != null
                ? context.GetTranslationFor(subject)
                : context.GetTranslationFor(member.DeclaringType);
        }

        #endregion

        private int GetTranslationSize()
        {
            return (_subjectTranslation != null)
                ? _subjectTranslation.TranslationSize + ".".Length + _memberName.Length
                : _memberName.Length;
        }

        private int GetFormattingSize()
            => _subjectTranslation?.FormattingSize ?? +_context.GetFormattingSize(TokenType.MethodName);

        public ExpressionType NodeType => ExpressionType.MemberAccess;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _subjectTranslation?.GetIndentSize() ?? 0;

        public int GetLineCount() => _subjectTranslation?.GetLineCount() ?? 1;

        public void WriteTo(TranslationWriter writer)
        {
            if (_subjectTranslation != null)
            {
                _subjectTranslation.WriteInParenthesesIfRequired(writer, _context);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_memberName);
        }
    }
}

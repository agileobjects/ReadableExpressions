namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberAccessTranslation : ITranslation
    {
        private readonly MemberExpression _memberAccess;
        private readonly ITranslation _subject;

        public MemberAccessTranslation(MemberExpression memberAccess, ITranslationContext context)
        {
            _memberAccess = memberAccess;
            _subject = GetSubjectOrNull(context);
            EstimatedSize = GetEstimatedSize();
        }

        private ITranslation GetSubjectOrNull(ITranslationContext context)
        {
            if (_memberAccess.Expression == null)
            {
                return context.GetTranslationFor(_memberAccess.Member.DeclaringType);
            }

            if (SubjectIsCapturedInstance())
            {
                return null;
            }

            return context.GetTranslationFor(_memberAccess.Expression);
        }

        private bool SubjectIsCapturedInstance()
        {
            if (_memberAccess.Expression.NodeType != ExpressionType.Constant)
            {
                return false;
            }

            var subjectType = ((ConstantExpression)_memberAccess.Expression).Type;

            return subjectType == _memberAccess.Member.DeclaringType;
        }

        private int GetEstimatedSize()
        {
            return (_subject != null)
                ? _subject.EstimatedSize + ".".Length + _memberAccess.Member.Name.Length
                : _memberAccess.Member.Name.Length;
        }

        public ExpressionType NodeType => ExpressionType.MemberAccess;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_subject != null)
            {
                if (CastTranslation.IsCast(_subject.NodeType))
                {
                    _subject.WriteInParentheses(context);
                }
                else
                {
                    _subject.WriteTo(context);
                }

                context.WriteToTranslation('.');
            }

            context.WriteToTranslation(_memberAccess.Member.Name);
        }
    }
}

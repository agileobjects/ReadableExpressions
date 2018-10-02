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
        private readonly ITranslationContext _context;
        private readonly ITranslation _subject;

        public MemberAccessTranslation(MemberExpression memberAccess, ITranslationContext context)
        {
            _memberAccess = memberAccess;
            _context = context;
            _subject = GetSubject(memberAccess, context);

            EstimatedSize = _subject.EstimatedSize + ".".Length + _memberAccess.Member.Name.Length;
        }

        private static ITranslation GetSubject(MemberExpression memberAccess, ITranslationContext context)
        {
            return (memberAccess.Expression != null)
                ? context.GetTranslationFor(memberAccess)
                : context.GetTranslationFor(memberAccess.Member.DeclaringType);
        }

        public int EstimatedSize { get; }

        public void WriteToTranslation()
        {
            _subject.WriteToTranslation();
            _context.WriteToTranslation('.');
            _context.WriteToTranslation(_memberAccess.Member.Name);
        }
    }
}

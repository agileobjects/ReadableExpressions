namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class MethodCallTranslation : ITranslation
    {
        private readonly MethodCallExpression _expression;
        private readonly ITranslationContext _context;
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public MethodCallTranslation(MethodCallExpression expression, ITranslationContext context)
        {
            _expression = expression;
            _context = context;

            _subject = context.GetTranslationFor(expression.GetSubject());
            _parameters = new ParameterSetTranslation(expression.Arguments, context);
            EstimatedSize = _subject.EstimatedSize + ".".Length + _parameters.EstimatedSize;
        }

        public int EstimatedSize { get; }

        public void WriteToTranslation()
        {
            _subject.WriteToTranslation();
            _context.WriteToTranslation('.');
            _context.WriteToTranslation(_expression.Method.Name);
            _parameters.WriteToTranslation();
        }
    }
}

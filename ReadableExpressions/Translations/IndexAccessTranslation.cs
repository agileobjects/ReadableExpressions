namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class IndexAccessTranslation : ITranslation
    {
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public IndexAccessTranslation(IndexExpression indexAccess, ITranslationContext context)
        {
            _subject = context.GetTranslationFor(indexAccess.Object);
            _parameters = new ParameterSetTranslation(indexAccess.Arguments, context);
            EstimatedSize = GetEstimatedSize();
        }

        public IndexAccessTranslation(ITranslation subject, ParameterSetTranslation parameters)
        {
            _subject = subject;
            _parameters = parameters;
            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize() => _subject.EstimatedSize + _parameters.EstimatedSize + 2;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _subject.WriteTo(context);
            context.WriteToTranslation('[');
            _parameters.WithoutParentheses().WriteTo(context);
            context.WriteToTranslation(']');
        }
    }
}
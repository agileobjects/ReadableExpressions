namespace AgileObjects.ReadableExpressions.Translations
{
    internal class IndexAccessTranslation : ITranslation
    {
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public IndexAccessTranslation(ITranslation subject, ParameterSetTranslation parameters)
        {
            _subject = subject;
            _parameters = parameters;

            EstimatedSize = _subject.EstimatedSize + _parameters.EstimatedSize + 2;
        }

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
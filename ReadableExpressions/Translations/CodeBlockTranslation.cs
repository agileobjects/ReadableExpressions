namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Linq.Expressions;

    internal class CodeBlockTranslation : ITranslation
    {
        private readonly ITranslation _translation;

        public CodeBlockTranslation(ITranslation translation)
        {
            _translation = translation;
            EstimatedSize = _translation.EstimatedSize + 6; // <- bit of extra padding
        }

        public ExpressionType NodeType => _translation.NodeType;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _translation.WriteTo(context);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Linq.Expressions;

    internal class ParenthesisedTranslation : ITranslation
    {
        private readonly ITranslation _translation;

        public ParenthesisedTranslation(ITranslation translation)
        {
            _translation = translation;
            EstimatedSize = _translation.EstimatedSize + 2;
        }

        public ExpressionType NodeType => _translation.NodeType;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
            => _translation.WriteInParentheses(context);
    }
}
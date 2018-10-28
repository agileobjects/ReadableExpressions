namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class TranslationWrapper : ITranslation
    {
        private readonly ITranslation _translation;
        private string _prefix;
        private string _suffix;

        public TranslationWrapper(ITranslation translation)
        {
            _translation = translation;
        }

        public ExpressionType NodeType => _translation.NodeType;

        public int EstimatedSize =>
            (_translation.EstimatedSize + _prefix?.Length + _suffix?.Length).GetValueOrDefault();

        public TranslationWrapper WithPrefix(string prefix)
        {
            _prefix = prefix;
            return this;
        }

        public TranslationWrapper WithSuffix(string suffix)
        {
            _suffix = suffix;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation(_prefix);
            _translation.WriteTo(context);
            context.WriteToTranslation(_suffix);
        }
    }
}
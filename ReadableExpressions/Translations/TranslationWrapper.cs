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
        private bool _hasPrefix;
        private string _suffix;
        private bool _hasSuffix;

        public TranslationWrapper(ITranslation translation)
        {
            _translation = translation;
        }

        public ExpressionType NodeType => _translation.NodeType;

        public int EstimatedSize => GetEstimatedSize();

        private int GetEstimatedSize()
        {
            var estimatedSize = _translation.EstimatedSize;

            if (_hasPrefix)
            {
                estimatedSize += _prefix.Length;
            }

            if (_hasSuffix)
            {
                estimatedSize += _suffix.Length;
            }

            return estimatedSize;
        }

        public TranslationWrapper WithPrefix(string prefix)
        {
            _prefix = prefix;
            _hasPrefix = true;
            return this;
        }

        public TranslationWrapper WithSuffix(string suffix)
        {
            _suffix = suffix;
            _hasSuffix = true;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_hasPrefix)
            {
                context.WriteToTranslation(_prefix);
            }

            _translation.WriteTo(context);

            if (_hasSuffix)
            {
                context.WriteToTranslation(_suffix);
            }
        }
    }
}
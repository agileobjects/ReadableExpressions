namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;
    using static TokenType;

    internal class TranslationWrapper : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslatable _translatable;
        private string _prefix;
        private TokenType _prefixTokenType;
        private bool _hasPrefix;
        private string _suffix;
        private TokenType _suffixTokenType;
        private bool _hasSuffix;

        public TranslationWrapper(ITranslation translation)
            : this(translation.NodeType, translation, translation.Type)
        {
        }

        public TranslationWrapper(ExpressionType nodeType, ITranslatable translatable, Type type)
        {
            NodeType = nodeType;
            Type = type;
            _translatable = translatable;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize => GetEstimatedSize();

        private int GetEstimatedSize()
        {
            var estimatedSize = _translatable.EstimatedSize;

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

        public bool IsTerminated => _translatable.IsTerminated();

        public TranslationWrapper WrappedWith(string prefix, string suffix) => WithPrefix(prefix).WithSuffix(suffix);

        public TranslationWrapper WithPrefix(string prefix, TokenType tokenType = Default)
        {
            _prefix = prefix;
            _prefixTokenType = tokenType;
            _hasPrefix = true;
            return this;
        }

        public TranslationWrapper WithSuffix(string suffix, TokenType tokenType = Default)
        {
            _suffix = suffix;
            _suffixTokenType = tokenType;
            _hasSuffix = true;
            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_hasPrefix)
            {
                buffer.WriteToTranslation(_prefix, _prefixTokenType);
            }

            _translatable.WriteTo(buffer);

            if (_hasSuffix)
            {
                buffer.WriteToTranslation(_suffix, _suffixTokenType);
            }
        }
    }
}
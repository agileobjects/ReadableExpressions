using System;

namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class TranslationWrapper : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslatable _translatable;
        private string _prefix;
        private bool _hasPrefix;
        private string _suffix;
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

            _translatable.WriteTo(context);

            if (_hasSuffix)
            {
                context.WriteToTranslation(_suffix);
            }
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class WrappedTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly string _prefix;
        private readonly ITranslation _translation;
        private readonly string _suffix;

        public WrappedTranslation(string prefix, ITranslation translation, string suffix)
        {
            _prefix = prefix;
            _translation = translation;
            _suffix = suffix;
            TranslationSize = prefix.Length + _translation.TranslationSize + suffix.Length;
        }

        public ExpressionType NodeType => _translation.NodeType;

        public Type Type => _translation.Type;

        public int TranslationSize { get; }

        public int FormattingSize => _translation.FormattingSize;

        public bool IsTerminated => _translation.IsTerminated();

        public int GetIndentSize() => _translation.GetIndentSize();

        public int GetLineCount() => _translation.GetLineCount();

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteToTranslation(_prefix);
            _translation.WriteTo(buffer);
            buffer.WriteToTranslation(_suffix);
        }
    }
}
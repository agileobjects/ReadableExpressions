namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class CodeBlockTranslation : ITranslation
    {
        private readonly ITranslation _translation;
        private bool _writeBraces;

        public CodeBlockTranslation(ITranslation translation)
        {
            NodeType = translation.NodeType;
            _translation = translation;
            _writeBraces = translation.IsMultiStatement();
            CalculateEstimatedSize();
        }

        private void CalculateEstimatedSize()
        {
            EstimatedSize = _translation.EstimatedSize;

            if (_writeBraces)
            {
                EstimatedSize += 10;
            }
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; private set; }

        public CodeBlockTranslation Unterminated()
        {
            return this;
        }

        public CodeBlockTranslation WithBraces()
        {
            if (_writeBraces)
            {
                return this;
            }

            _writeBraces = true;
            CalculateEstimatedSize();
            return this;
        }

        public CodeBlockTranslation WithoutBraces()
        {
            if (_writeBraces == false)
            {
                return this;
            }

            _writeBraces = false;
            CalculateEstimatedSize();
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_writeBraces)
            {
                context.WriteOpeningBraceToTranslation();
            }

            _translation.WriteTo(context);

            if (_writeBraces)
            {
                context.WriteClosingBraceToTranslation();
            }
        }
    }
}
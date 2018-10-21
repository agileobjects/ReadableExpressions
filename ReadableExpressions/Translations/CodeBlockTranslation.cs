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
        private bool _ensureTerminated;
        private bool _ensureReturnKeyword;
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

            if (_ensureReturnKeyword)
            {
                EstimatedSize += 10;
            }

            if (_writeBraces)
            {
                EstimatedSize += 10;
            }
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; private set; }

        public CodeBlockTranslation Terminated()
        {
            _ensureTerminated = true;
            return this;
        }

        public CodeBlockTranslation Unterminated()
        {
            return this;
        }

        public CodeBlockTranslation WithReturnKeyword()
        {
            _ensureReturnKeyword = true;
            CalculateEstimatedSize();
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

                if (WriteEmptyCodeBlock(context))
                {
                    return;
                }
            }

            if (_ensureReturnKeyword && !_translation.IsMultiStatement())
            {
                context.WriteToTranslation("return ");
            }

            _translation.WriteTo(context);

            if (EnsureTerminated())
            {
                context.WriteToTranslation(';');
            }

            if (_writeBraces)
            {
                context.WriteClosingBraceToTranslation();
            }
        }

        private bool WriteEmptyCodeBlock(ITranslationContext context)
        {
            if ((_translation is IPotentialEmptyTranslatable emptyTranslatable) && emptyTranslatable.IsEmpty)
            {
                context.Unindent();
                context.WriteToTranslation('}');
                return true;
            }

            return false;
        }

        private bool EnsureTerminated() => _ensureTerminated && (_translation.IsTerminated() == false);
    }
}
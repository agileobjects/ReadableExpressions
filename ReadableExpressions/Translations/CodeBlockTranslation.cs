namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class CodeBlockTranslation :
        ITranslation,
        IPotentialMultiStatementTranslatable,
        IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslation _translation;
        private bool _ensureTerminated;
        private bool _ensureReturnKeyword;
        private bool _startOnSameLine;
        private bool _formatAsSingleLambdaParameter;
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

        public bool IsMultiStatement => _translation.IsMultiStatement();

        public bool IsTerminated => _ensureTerminated || _translation.IsTerminated();

        public bool HasBraces => _writeBraces;

        public CodeBlockTranslation WithTermination()
        {
            _ensureTerminated = true;
            return this;
        }

        public CodeBlockTranslation WithoutTermination()
        {
            _ensureTerminated = false;
            return this;
        }

        public CodeBlockTranslation WithSingleLamdaParameterFormatting()
        {
            _formatAsSingleLambdaParameter = true;
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

        public CodeBlockTranslation WithoutStartingNewLine()
        {
            _startOnSameLine = true;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_writeBraces)
            {
                context.WriteOpeningBraceToTranslation(
                    startOnNewLine: _startOnSameLine == false && _formatAsSingleLambdaParameter == false);

                if (WriteEmptyCodeBlock(context))
                {
                    return;
                }
            }

            if (_formatAsSingleLambdaParameter)
            {
                context.Indent();
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

            if (_formatAsSingleLambdaParameter)
            {
                context.Unindent();
            }
        }

        private bool WriteEmptyCodeBlock(ITranslationContext context)
        {
            if ((_translation is IPotentialEmptyTranslatable emptyTranslatable) && emptyTranslatable.IsEmpty)
            {
                context.WriteClosingBraceToTranslation(startOnNewLine: false);
                return true;
            }

            return false;
        }

        private bool EnsureTerminated() => _ensureTerminated && (_translation.IsTerminated() == false);
    }
}
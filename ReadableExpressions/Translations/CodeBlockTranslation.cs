namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

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

        public Type Type => _translation.Type;

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

            if ((_writeBraces == false) && (NodeType == ExpressionType.Block))
            {
                ((BlockTranslation)_translation).WithoutTermination();
            }

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

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_writeBraces)
            {
                buffer.WriteOpeningBraceToTranslation(
                    startOnNewLine: _startOnSameLine == false && _formatAsSingleLambdaParameter == false);

                if (WriteEmptyCodeBlock(buffer))
                {
                    return;
                }
            }

            if (_formatAsSingleLambdaParameter)
            {
                buffer.Indent();
            }

            if (_writeBraces && _ensureReturnKeyword && !_translation.IsMultiStatement())
            {
                buffer.WriteToTranslation("return ");
            }

            _translation.WriteTo(buffer);

            if (EnsureTerminated())
            {
                buffer.WriteToTranslation(';');
            }

            if (_writeBraces)
            {
                buffer.WriteClosingBraceToTranslation();
            }

            if (_formatAsSingleLambdaParameter)
            {
                buffer.Unindent();
            }
        }

        private bool WriteEmptyCodeBlock(TranslationBuffer buffer)
        {
            if ((_translation is IPotentialEmptyTranslatable emptyTranslatable) && emptyTranslatable.IsEmpty)
            {
                buffer.WriteClosingBraceToTranslation(startOnNewLine: false);
                return true;
            }

            return false;
        }

        private bool EnsureTerminated() => _ensureTerminated && (_translation.IsTerminated() == false);
    }
}
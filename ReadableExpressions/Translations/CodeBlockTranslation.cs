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
        private bool _startOnNewLine;
        private bool _indentContents;
        private bool _writeBraces;

        public CodeBlockTranslation(ITranslation translation)
        {
            NodeType = translation.NodeType;
            _translation = translation;
            _startOnNewLine = true;
            _writeBraces = IsMultiStatement = translation.IsMultiStatement();
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

        public bool IsMultiStatement { get; }

        public bool IsTerminated => _ensureTerminated || _translation.IsTerminated();

        public bool HasBraces => _writeBraces;

        public bool IsMultiStatementLambda(ITranslationContext context)
        {
            switch (NodeType)
            {
                case ExpressionType.Lambda:
                case ExpressionType.Quote when context.Settings.DoNotCommentQuotedLambdas:
                    return IsMultiStatement;
            }

            return false;
        }

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

        public CodeBlockTranslation WithSingleCodeBlockParameterFormatting()
        {
            _startOnNewLine = false;
            _indentContents = true;
            return this;
        }

        public CodeBlockTranslation WithSingleLamdaParameterFormatting()
        {
            _startOnNewLine = false;
            return WithoutBraces();
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
            _startOnNewLine = false;
            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_writeBraces)
            {
                buffer.WriteOpeningBraceToTranslation(_startOnNewLine);

                if (WriteEmptyCodeBlock(buffer))
                {
                    return;
                }
            }

            if (_indentContents)
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

            if (_indentContents)
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
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
        private bool _isEmptyTranslation;
        private readonly ITranslationContext _context;
        private bool _ensureTerminated;
        private bool _ensureReturnKeyword;
        private bool _startOnNewLine;
        private bool _indentContents;
        private bool _writeBraces;

        public CodeBlockTranslation(ITranslation translation, ITranslationContext context)
        {
            NodeType = translation.NodeType;
            _translation = translation;
            _isEmptyTranslation = translation is IPotentialEmptyTranslatable empty && empty.IsEmpty;
            _context = context;
            _startOnNewLine = true;
            _writeBraces = IsMultiStatement = translation.IsMultiStatement();
            CalculateSizes();
        }

        private void CalculateSizes()
        {
            var translationSize = _translation.TranslationSize;
            var formattingSize = _translation.FormattingSize;

            if (_ensureReturnKeyword)
            {
                translationSize += 10;
                formattingSize += _context.GetControlStatementFormattingSize();
            }

            if (_writeBraces)
            {
                translationSize += 10;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type => _translation.Type;

        public int TranslationSize { get; private set; }

        public int FormattingSize { get; private set; }

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

        public void WithSingleLamdaParameterFormatting()
        {
            _startOnNewLine = false;
            WithoutBraces();
        }

        public CodeBlockTranslation WithReturnKeyword()
        {
            _ensureReturnKeyword = true;
            CalculateSizes();
            return this;
        }

        public CodeBlockTranslation WithBraces()
        {
            if (_writeBraces)
            {
                return this;
            }

            _writeBraces = true;
            CalculateSizes();
            return this;
        }

        public CodeBlockTranslation WithoutBraces()
        {
            if (_writeBraces == false)
            {
                return this;
            }

            _writeBraces = false;
            CalculateSizes();
            return this;
        }

        public CodeBlockTranslation WithoutStartingNewLine()
        {
            _startOnNewLine = false;
            return this;
        }

        public IParameterTranslation AsParameterTranslation()
            => _translation as IParameterTranslation;

        public int GetLineCount()
        {
            if (_isEmptyTranslation)
            {
                return _writeBraces ? _startOnNewLine ? 3 : 2 : 0;
            }

            var translationLineCount = _translation.GetLineCount();

            if (_writeBraces)
            {
                translationLineCount += _startOnNewLine ? 3 : 2;
            }

            return translationLineCount;
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
                buffer.WriteReturnToTranslation();
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
            if (_isEmptyTranslation)
            {
                buffer.WriteClosingBraceToTranslation(startOnNewLine: false);
                return true;
            }

            return false;
        }

        private bool EnsureTerminated() => _ensureTerminated && (_translation.IsTerminated() == false);
    }
}
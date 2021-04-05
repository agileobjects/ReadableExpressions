﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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
        private readonly bool _isEmptyTranslation;
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

        public int GetIndentSize()
        {
            if (_isEmptyTranslation)
            {
                return 0;
            }

            var indentSize = _translation.GetIndentSize();

            if (_indentContents || _writeBraces)
            {
                var indentLength = _context.Settings.IndentLength;
                var translationIndentSize = _translation.GetLineCount() * indentLength;

                indentSize += translationIndentSize;

                if (_writeBraces)
                {
                    indentSize += 2 * indentLength;

                    if (_indentContents)
                    {
                        indentSize += translationIndentSize;
                    }
                }
            }

            return indentSize;
        }

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

        public void WriteTo(TranslationWriter writer)
        {
            if (_writeBraces)
            {
                writer.WriteOpeningBraceToTranslation(_startOnNewLine);

                if (WriteEmptyCodeBlock(writer))
                {
                    return;
                }
            }

            if (_indentContents)
            {
                writer.Indent();
            }

            if (_writeBraces && _ensureReturnKeyword && !_translation.IsMultiStatement())
            {
                writer.WriteReturnToTranslation();
            }

            _translation.WriteTo(writer);

            if (EnsureTerminated())
            {
                writer.WriteToTranslation(';');
            }

            if (_writeBraces)
            {
                writer.WriteClosingBraceToTranslation();
            }

            if (_indentContents)
            {
                writer.Unindent();
            }
        }

        private bool WriteEmptyCodeBlock(TranslationWriter writer)
        {
            if (_isEmptyTranslation)
            {
                writer.WriteClosingBraceToTranslation(startOnNewLine: false);
                return true;
            }

            return false;
        }

        private bool EnsureTerminated() => _ensureTerminated && (_translation.IsTerminated() == false);
    }
}
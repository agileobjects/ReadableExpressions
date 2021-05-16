namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    /// <summary>
    /// An <see cref="ITranslation"/> for a potentiall multi-line source code block, providing
    /// methods to control formatting and output.
    /// </summary>
    public class CodeBlockTranslation :
        ITranslation,
        IPotentialMultiStatementTranslatable,
        IPotentialSelfTerminatingTranslatable,
        IPotentialGotoTranslatable
    {
        private readonly ITranslation _translation;
        private readonly bool _isEmptyTranslation;
        private readonly ITranslationContext _context;
        private bool _ensureTerminated;
        private bool _ensureReturnKeyword;
        private bool _startOnNewLine;
        private bool _indentContents;
        private bool _writeBraces;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBlockTranslation"/> class.
        /// </summary>
        /// <param name="translation">The <see cref="ITranslation"/> to create as a code block.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> in which the translation is being performed.
        /// </param>
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

        /// <inheritdoc />
        public ExpressionType NodeType { get; }

        /// <inheritdoc />
        public Type Type => _translation.Type;

        /// <inheritdoc />
        public int TranslationSize { get; private set; }

        /// <inheritdoc />
        public int FormattingSize { get; private set; }

        /// <summary>
        /// Gets a value indicating if this <see cref="CodeBlockTranslation"/> contains multiple
        /// statements.
        /// </summary>
        public bool IsMultiStatement { get; }

        /// <summary>
        /// Gets a value indicating if this <see cref="CodeBlockTranslation"/> ends in a terminating
        /// character.
        /// </summary>
        public bool IsTerminated => _ensureTerminated || _translation.IsTerminated();

        bool IPotentialGotoTranslatable.HasGoto => _translation.HasGoto();

        /// <summary>
        /// Gets a value indicating if this <see cref="CodeBlockTranslation"/> will produce output
        /// surrounded in curly braces.
        /// </summary>
        public bool HasBraces => _writeBraces;

        /// <summary>
        /// Gets a value indicating if this <see cref="CodeBlockTranslation"/> represents a
        /// multi-statement LambdaExpression.
        /// </summary>
        public bool IsMultiStatementLambda
        {
            get
            {
                switch (NodeType)
                {
                    case ExpressionType.Lambda:
                    case ExpressionType.Quote when _context.Settings.DoNotCommentQuotedLambdas:
                        return IsMultiStatement;
                }

                return false;
            }
        }

        /// <summary>
        /// Ensures this <see cref="CodeBlockTranslation"/> is ended with a terminating character.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public CodeBlockTranslation WithTermination()
        {
            _ensureTerminated = true;
            return this;
        }

        /// <summary>
        /// Ensures this <see cref="CodeBlockTranslation"/> is not ended with a terminating character.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public CodeBlockTranslation WithoutTermination()
        {
            _ensureTerminated = false;

            if ((_writeBraces == false) && (NodeType == ExpressionType.Block))
            {
                ((BlockTranslation)_translation).WithoutTermination();
            }

            return this;
        }

        /// <summary>
        /// Ensures this <see cref="CodeBlockTranslation"/> is formatted as a single-parameter code
        /// block.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public CodeBlockTranslation WithSingleCodeBlockParameterFormatting()
        {
            _startOnNewLine = false;
            _indentContents = true;
            return this;
        }

        /// <summary>
        /// Ensures this <see cref="CodeBlockTranslation"/> is formatted as a single-parameter lambda.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public void WithSingleLamdaParameterFormatting()
        {
            _startOnNewLine = false;
            WithoutBraces();
        }

        /// <summary>
        /// Ensures the final statement in this <see cref="CodeBlockTranslation"/> includes a return keyword.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public CodeBlockTranslation WithReturnKeyword()
        {
            _ensureReturnKeyword = true;
            CalculateSizes();
            return this;
        }

        /// <summary>
        /// Ensures the output of this <see cref="CodeBlockTranslation"/> is wrapped in curly braces.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
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

        /// <summary>
        /// Ensures the output of this <see cref="CodeBlockTranslation"/> is not wrapped in curly braces.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
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

        /// <summary>
        /// Ensures the output of this <see cref="CodeBlockTranslation"/> does not start on a new line.
        /// </summary>
        /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
        public CodeBlockTranslation WithoutStartingNewLine()
        {
            _startOnNewLine = false;
            return this;
        }

        internal IParameterTranslation AsParameterTranslation()
            => _translation as IParameterTranslation;

        internal INullKeywordTranslation AsNullKeywordTranslation()
            => _translation as INullKeywordTranslation;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
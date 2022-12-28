namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

/// <summary>
/// An <see cref="INodeTranslation"/> for a potentially multi-line source code block, providing
/// methods to control formatting and output.
/// </summary>
public class CodeBlockTranslation :
    INodeTranslation,
    IPotentialMultiStatementTranslatable,
    IPotentialSelfTerminatingTranslation,
    IPotentialGotoTranslation
{
    private readonly INodeTranslation _translation;
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
    /// <param name="translation">The <see cref="INodeTranslation"/> to create as a code block.</param>
    /// <param name="context">
    /// The <see cref="ITranslationContext"/> in which the translation is being performed.
    /// </param>
    public CodeBlockTranslation(
        INodeTranslation translation,
        ITranslationContext context)
    {
        NodeType = translation.NodeType;
        _translation = translation;
        _isEmptyTranslation = translation is IPotentialEmptyTranslation { IsEmpty: true };
        _context = context;
        _startOnNewLine = true;
        _writeBraces = IsMultiStatement = translation.IsMultiStatement();
    }

    /// <inheritdoc />
    public ExpressionType NodeType { get; }

    /// <inheritdoc />
    public int TranslationLength
    {
        get
        {
            var translationLength = _translation.TranslationLength;

            if (_ensureReturnKeyword)
            {
                translationLength += 10;
            }

            if (_writeBraces)
            {
                translationLength += 10;
            }

            return translationLength;
        }
    }

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

    bool IPotentialGotoTranslation.HasGoto => _translation.HasGoto();

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
            return NodeType switch
            {
                ExpressionType.Lambda => IsMultiStatement,
                ExpressionType.Quote when _context.Settings.DoNotCommentQuotedLambdas => IsMultiStatement,
                _ => false
            };
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

        if (_writeBraces == false && NodeType == ExpressionType.Block)
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
    public CodeBlockTranslation WithSingleLamdaParameterFormatting()
    {
        _startOnNewLine = false;
        WithoutBraces();
        return this;
    }

    /// <summary>
    /// Ensures the final statement in this <see cref="CodeBlockTranslation"/> includes a return keyword.
    /// </summary>
    /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
    public CodeBlockTranslation WithReturnKeyword()
    {
        _ensureReturnKeyword = true;
        return this;
    }

    /// <summary>
    /// Ensures the output of this <see cref="CodeBlockTranslation"/> is wrapped in curly braces.
    /// </summary>
    /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
    public CodeBlockTranslation WithBraces()
    {
        _writeBraces = true;
        return this;
    }

    /// <summary>
    /// Ensures the output of this <see cref="CodeBlockTranslation"/> is not wrapped in curly braces.
    /// </summary>
    /// <returns>This <see cref="CodeBlockTranslation"/>, to support a fluent API.</returns>
    public CodeBlockTranslation WithoutBraces()
    {
        _writeBraces = false;
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
            writer.WriteSemiColonToTranslation();
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

    private bool EnsureTerminated()
        => _ensureTerminated && _translation.IsTerminated() == false;
}
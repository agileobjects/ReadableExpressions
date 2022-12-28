namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

/// <summary>
/// An <see cref="INodeTranslation"/> which writes a use of an operator.
/// </summary>
public abstract class UnaryOperatorTranslationBase : INodeTranslation
{
    private readonly string _operator;
    private readonly ITranslation _operandTranslation;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnaryOperatorTranslationBase"/> class.
    /// </summary>
    /// <param name="operator">The name of the operator being applied.</param>
    /// <param name="operandTranslation">
    /// The <see cref="ITranslation"/> which will write the symbol to which the operator is
    /// being applied.
    /// </param>
    protected UnaryOperatorTranslationBase(
        string @operator,
        ITranslation operandTranslation)
    {
        _operator = @operator;
        _operandTranslation = operandTranslation;
    }

    /// <summary>
    /// Gets the ExpressionType of the translated operator use - ExpressionType.Extension.
    /// </summary>
    public virtual ExpressionType NodeType => ExpressionType.Extension;

    /// <inheritdoc />
    public int TranslationLength
        => _operator.Length + "()".Length + _operandTranslation.TranslationLength;

    /// <inheritdoc />
    public virtual void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation(_operator);
        _operandTranslation.WriteInParentheses(writer);
    }
}
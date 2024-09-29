namespace AgileObjects.ReadableExpressions.Translations;

using Formatting;

/// <summary>
/// An <see cref="INodeTranslation"/> with a fixed string value.
/// </summary>
public class FixedValueTranslation : INodeTranslation
{
    private readonly string _value;
    private readonly TokenType _tokenType;
    private readonly bool _isEmptyValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedValueTranslation"/> class.
    /// </summary>
    /// <param name="expression">The Expression translated by the <see cref="FixedValueTranslation"/>.</param>
    public FixedValueTranslation(Expression expression) :
        this(expression.NodeType, expression.ToString(), TokenType.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedValueTranslation"/> class.
    /// </summary>
    /// <param name="expressionType">The ExpressionType of the <see cref="FixedValueTranslation"/>.</param>
    /// <param name="value">The fixed, translated value.</param>
    /// <param name="tokenType">
    /// The <see cref="TokenType"/> with which the <see cref="FixedValueTranslation"/> should be
    /// written to the translation output.
    /// </param>
    public FixedValueTranslation(
        ExpressionType expressionType,
        string value,
        TokenType tokenType)
    {
        NodeType = expressionType;
        _value = value;
        _tokenType = tokenType;
        _isEmptyValue = string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Gets the ExpressionType of this <see cref="FixedValueTranslation"/>.
    /// </summary>
    public ExpressionType NodeType { get; }

    int ITranslation.TranslationLength => _value.Length;

    void ITranslation.WriteTo(TranslationWriter writer)
    {
        if (!_isEmptyValue)
        {
            writer.WriteToTranslation(_value, _tokenType);
        }
    }
}
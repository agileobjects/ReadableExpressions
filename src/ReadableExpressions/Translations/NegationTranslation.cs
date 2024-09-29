namespace AgileObjects.ReadableExpressions.Translations;

internal class NegationTranslation : INodeTranslation
{
    private const char _bang = '!';
    private readonly ITranslationContext _context;
    private readonly char _operator;
    private readonly INodeTranslation _negatedValue;

    public NegationTranslation(
        UnaryExpression negation,
        ITranslationContext context)
        : this(
            negation.NodeType,
            negation.NodeType == ExpressionType.Not ? _bang : '-',
            context.GetTranslationFor(negation.Operand),
            context)
    {
    }

    private NegationTranslation(
        ExpressionType negationType,
        char @operator,
        INodeTranslation negatedValue,
        ITranslationContext context)
    {
        _context = context;
        NodeType = negationType;
        _operator = @operator;
        _negatedValue = negatedValue;
    }

    public static INodeTranslation ForNot(
        INodeTranslation negatedValue,
        ITranslationContext context)
    {
        return new NegationTranslation(ExpressionType.Not, _bang, negatedValue, context);
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength => _negatedValue.TranslationLength + 3;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteToTranslation(_operator);
        _negatedValue.WriteInParenthesesIfRequired(writer, _context);
    }
}
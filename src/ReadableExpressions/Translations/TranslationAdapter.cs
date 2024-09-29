namespace AgileObjects.ReadableExpressions.Translations;

internal class TranslationAdapter :
    INodeTranslation,
    IPotentialSelfTerminatingTranslation
{
    private readonly ITranslation _baseTranslation;

    public TranslationAdapter(
        ITranslation baseTranslation,
        ExpressionType nodeType)
    {
        _baseTranslation = baseTranslation;
        NodeType = nodeType;
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength => _baseTranslation.TranslationLength;

    public bool IsTerminated => _baseTranslation.IsTerminated();

    public void WriteTo(TranslationWriter writer) =>
        _baseTranslation.WriteTo(writer);
}
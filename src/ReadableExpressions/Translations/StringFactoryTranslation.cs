namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal class StringFactoryTranslation : INodeTranslation
{
    private readonly ExpressionTranslation _context;
    private readonly SourceCodeTranslationFactory _translationFactory;
    private readonly Expression _expression;
    private string _translation;

    public StringFactoryTranslation(
        Expression expression,
        SourceCodeTranslationFactory translationFactory,
        ExpressionTranslation context)
    {
        _context = context;
        _translationFactory = translationFactory;
        _expression = expression;
    }

    public ExpressionType NodeType => _expression.NodeType;

    public int TranslationLength => GetTranslation()?.Length ?? 0;

    public void WriteTo(TranslationWriter writer) =>
        writer.WriteToTranslation(GetTranslation());

    private string GetTranslation()
    {
        return _translation ??= _translationFactory
            .Invoke(_expression, expression => _context
                .GetDefaultTranslation(expression)?
                .WriteUsing(_context.Settings));
    }
}
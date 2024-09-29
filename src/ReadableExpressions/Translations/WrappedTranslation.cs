namespace AgileObjects.ReadableExpressions.Translations;

internal class WrappedTranslation :
    INodeTranslation,
    IPotentialSelfTerminatingTranslation
{
    private readonly string _prefix;
    private readonly INodeTranslation _translation;
    private readonly string _suffix;

    public WrappedTranslation(
        string prefix,
        INodeTranslation translation,
        string suffix)
    {
        _prefix = prefix;
        _translation = translation;
        _suffix = suffix;
    }

    public ExpressionType NodeType => _translation.NodeType;

    public int TranslationLength => 
        _prefix.Length + _translation.TranslationLength + _suffix.Length;

    public bool IsTerminated => _translation.IsTerminated();

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteToTranslation(_prefix);
        _translation.WriteTo(writer);
        writer.WriteToTranslation(_suffix);
    }
}
namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

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

    public void WriteTo(TranslationWriter writer)
        => _baseTranslation.WriteTo(writer);
}
#if FEATURE_VALUE_TUPLE
namespace AgileObjects.ReadableExpressions.Translations;

internal class ValueTupleNewingTranslation :
    NewingTranslationBase,
    INodeTranslation
{
    public ValueTupleNewingTranslation(
        NewExpression newing,
        ITranslationContext context) :
        base(newing, context)
    {
        Parameters.WithParentheses();
    }

    public int TranslationLength => Parameters.TranslationLength + "()".Length;

    public void WriteTo(TranslationWriter writer)
        => Parameters.WriteTo(writer);
}
#endif
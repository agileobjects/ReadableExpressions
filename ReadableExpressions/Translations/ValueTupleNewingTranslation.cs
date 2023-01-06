#if FEATURE_VALUE_TUPLE
namespace AgileObjects.ReadableExpressions.Translations;

using System.Linq.Expressions;

internal class ValueTupleNewingTranslation : NewingTranslationBase, INodeTranslation
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
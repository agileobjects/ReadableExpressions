namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;
using NetStandardPolyfills;

internal class NewingTranslation : NewingTranslationBase, INodeTranslation
{
    private readonly INodeTranslation _typeNameTranslation;

    private NewingTranslation(
        NewExpression newing,
        ITranslationContext context,
        bool omitParenthesesIfParameterless) :
        base(newing, context)
    {
        _typeNameTranslation = context
            .GetTranslationFor(newing.Type)
            .WithObjectTypeName();

        if (omitParenthesesIfParameterless && Parameters.None)
        {
            Parameters.WithoutParentheses();
        }
        else
        {
            Parameters.WithParentheses();
        }
    }

    public static INodeTranslation For(
        NewExpression newing,
        ITranslationContext context,
        bool omitParenthesesIfParameterless = false)
    {
        if (newing.Type.IsAnonymous())
        {
            return new AnonymousTypeNewingTranslation(newing, context);
        }

#if FEATURE_VALUE_TUPLE
        if (newing.Type.IsValueTuple() && newing.Arguments.Any())
        {
            return new ValueTupleNewingTranslation(newing, context);
        }
#endif
        return new NewingTranslation(newing, context, omitParenthesesIfParameterless);
    }

    public int TranslationLength =>
        "new ".Length +
        _typeNameTranslation.TranslationLength +
         Parameters.TranslationLength;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteNewToTranslation();
        _typeNameTranslation.WriteTo(writer);
        Parameters.WriteTo(writer);
    }
}
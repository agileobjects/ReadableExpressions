namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal class NewArrayTranslation : INodeTranslation
{
    private readonly INodeTranslation _typeNameTranslation;
    private readonly INodeTranslation[] _boundTranslations;
    private readonly int _boundTranslationCount;

    public NewArrayTranslation(
        NewArrayExpression newArray,
        ITranslationContext context)
    {
        _typeNameTranslation = context
            .GetTranslationFor(newArray.Type.GetElementType());

        if (newArray.Expressions.None())
        {
            _boundTranslations = Enumerable<INodeTranslation>.EmptyArray;
        }
        else
        {
            _boundTranslationCount = newArray.Expressions.Count;
            _boundTranslations = new INodeTranslation[_boundTranslationCount];

            for (var i = 0; i < _boundTranslationCount; ++i)
            {
                _boundTranslations[i] = context
                    .GetTranslationFor(newArray.Expressions[i]);
            }
        }
    }

    public ExpressionType NodeType => ExpressionType.NewArrayBounds;

    public int TranslationLength =>
        _typeNameTranslation.TranslationLength + 6 +
        _boundTranslations.TotalTranslationLength();

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteNewToTranslation();
        _typeNameTranslation.WriteTo(writer);
        writer.WriteToTranslation('[');

        if (_boundTranslationCount != 0)
        {
            for (var i = 0; ;)
            {
                _boundTranslations[i].WriteTo(writer);

                ++i;

                if (i == _boundTranslationCount)
                {
                    break;
                }

                writer.WriteToTranslation("[]");
            }
        }

        writer.WriteToTranslation(']');
    }
}
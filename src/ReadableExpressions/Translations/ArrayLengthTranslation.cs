namespace AgileObjects.ReadableExpressions.Translations;

internal class ArrayLengthTranslation : INodeTranslation
{
    private const string _length = ".Length";
    private const int _lengthPropertyLength = 7;

    private readonly INodeTranslation _operand;

    public ArrayLengthTranslation(
        UnaryExpression arrayLength,
        ITranslationContext context)
    {
        _operand = context.GetTranslationFor(arrayLength.Operand);
    }

    public ExpressionType NodeType => ExpressionType.ArrayLength;

    public int TranslationLength => _operand.TranslationLength + _lengthPropertyLength;

    public void WriteTo(TranslationWriter writer)
    {
        _operand.WriteTo(writer);
        writer.WriteToTranslation(_length);
    }
}
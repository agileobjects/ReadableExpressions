namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal class ArrayLengthTranslation : INodeTranslation
{
    private const string _length = ".Length";
    private static readonly int _lengthPropertyLength = _length.Length;

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
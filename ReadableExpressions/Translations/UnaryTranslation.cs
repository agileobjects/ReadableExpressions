namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
using static Microsoft.Scripting.Ast.ExpressionType;
#else
using System.Linq.Expressions;
using static System.Linq.Expressions.ExpressionType;
#endif

internal class UnaryTranslation : INodeTranslation
{
    private readonly string _operator;
    private readonly ITranslation _operandTranslation;
    private readonly bool _operatorIsSuffix;

    public UnaryTranslation(UnaryExpression unary, ITranslationContext context)
    {
        NodeType = unary.NodeType;
        _operator = GetOperatorFor(unary.NodeType);

        switch (NodeType)
        {
            case PostDecrementAssign:
            case PostIncrementAssign:
                _operatorIsSuffix = true;
                break;
        }

        _operandTranslation = context.GetTranslationFor(unary.Operand);
    }

    private static string GetOperatorFor(ExpressionType nodeType)
    {
        return nodeType switch
        {
            Decrement => "--",
            Increment => "++",
            IsTrue => string.Empty,
            IsFalse => "!",
            OnesComplement => "~",
            PostDecrementAssign => "--",
            PostIncrementAssign => "++",
            PreDecrementAssign => "--",
            PreIncrementAssign => "++",
            _ => "+"
        };
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength
        => _operandTranslation.TranslationLength + _operator.Length;

    public void WriteTo(TranslationWriter writer)
    {
        if (_operatorIsSuffix == false)
        {
            writer.WriteToTranslation(_operator);
        }

        _operandTranslation?.WriteTo(writer);

        if (_operatorIsSuffix)
        {
            writer.WriteToTranslation(_operator);
        }
    }
}
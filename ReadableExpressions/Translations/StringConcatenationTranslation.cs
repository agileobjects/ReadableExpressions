namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;
#if NET35
using static Microsoft.Scripting.Ast.ExpressionType;
#else
using static System.Linq.Expressions.ExpressionType;
#endif

internal class StringConcatenationTranslation : INodeTranslation
{
    private readonly int _operandCount;
    private readonly IList<INodeTranslation> _operandTranslations;

    private StringConcatenationTranslation(
        ExpressionType nodeType,
        int operandCount,
        IList<Expression> operands,
        ITranslationContext context)
    {
        NodeType = nodeType;

        _operandCount = operandCount;
        _operandTranslations = new INodeTranslation[operandCount];

        for (var i = 0; i < operandCount; ++i)
        {
            var operand = operands[i];

            if (operand.NodeType == Call)
            {
                var methodCall = (MethodCallExpression)operand;

                if (methodCall.Method.Name == nameof(ToString) &&
                    methodCall.Arguments.None())
                {
                    operand = methodCall.GetSubject();
                }
            }

            var operandTranslation = context.GetTranslationFor(operand);

            if (operand.Type != typeof(string) && operandTranslation.IsBinary())
            {
                operandTranslation = operandTranslation.WithParentheses();
            }

            _operandTranslations[i] = operandTranslation;
        }
    }

    public static INodeTranslation ForAddition(
        BinaryExpression addition,
        ITranslationContext context)
    {
        return new StringConcatenationTranslation(
            Add,
            operandCount: 2,
            new[] { addition.Left, addition.Right },
            context);
    }

    public static bool TryCreateForConcatCall(
        MethodCallExpression methodCall,
        ITranslationContext context,
        out INodeTranslation concatTranslation)
    {
        if (!IsStringConcatCall(methodCall))
        {
            concatTranslation = null;
            return false;
        }

        var operands = methodCall.Arguments;
        var operandCount = operands.Count;

        if (operandCount == 1 && operands.First().NodeType == NewArrayInit)
        {
            operands = ((NewArrayExpression)operands.First()).Expressions;
            operandCount = operands.Count;
        }

        concatTranslation =
            new StringConcatenationTranslation(Call, operandCount, operands, context);

        return true;
    }

    private static bool IsStringConcatCall(MethodCallExpression methodCall)
    {
        return methodCall.Method.IsStatic &&
               methodCall.Method.DeclaringType == typeof(string) &&
               methodCall.Method.Name == nameof(string.Concat);
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength 
        => _operandTranslations.TotalTranslationLength(separator: " + ");

    public void WriteTo(TranslationWriter writer)
    {
        for (var i = 0; ;)
        {
            var operandTranslation = _operandTranslations[i];

            if (operandTranslation.NodeType == Conditional || 
                operandTranslation.IsAssignment())
            {
                operandTranslation.WriteInParentheses(writer);
            }
            else
            {
                operandTranslation.WriteTo(writer);
            }

            ++i;

            if (i == _operandCount)
            {
                break;
            }

            writer.WriteToTranslation(" + ");
        }
    }
}
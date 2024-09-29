﻿namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using static ExpressionType;

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

        var operandTypes = new OperandType[operandCount];
        var hasAllStringOperands = true;
        var hasSingleToStringCall = true;
        var firstToStringCallIndex = -1;

        for (var i = 0; i < operandCount; ++i)
        {
            var operand = operands[i];

            if (operand.Type != typeof(string))
            {
                operandTypes[i] = OperandType.NonString;
                hasAllStringOperands = false;
                continue;
            }

            if (operand.NodeType == Call)
            {
                var methodCall = (MethodCallExpression)operand;

                if (methodCall.Method.Name == nameof(ToString) &&
                    methodCall.Arguments.None())
                {
                    operandTypes[i] = OperandType.ToStringCall;

                    if (firstToStringCallIndex == -1)
                    {
                        firstToStringCallIndex = i;
                        continue;
                    }

                    hasSingleToStringCall = false;
                    continue;
                }
            }

            operandTypes[i] = OperandType.String;
        }

        for (var i = 0; i < operandCount; ++i)
        {
            var operand = operands[i];

            if (hasAllStringOperands &&
               (hasSingleToStringCall || i > firstToStringCallIndex) &&
                operandTypes[i] == OperandType.ToStringCall)
            {
                operand = ((MethodCallExpression)operand).GetSubject();
            }

            _operandTranslations[i] = context.GetTranslationFor(operand);
        }
    }

    #region Factory Methods

    public static INodeTranslation ForAddition(
        BinaryExpression addition,
        ITranslationContext context)
    {
        var flattenedOperands = FlattenOperands(addition).ToList();

        return new StringConcatenationTranslation(
            Add,
            operandCount: flattenedOperands.Count,
            flattenedOperands,
            context);
    }

    private static IEnumerable<Expression> FlattenOperands(
        BinaryExpression binary)
    {
        foreach (var operand in FlattenOperands(binary.Left))
        {
            yield return operand;
        }

        foreach (var operand in FlattenOperands(binary.Right))
        {
            yield return operand;
        }
    }

    private static IEnumerable<Expression> FlattenOperands(Expression expression)
    {
        if (expression.NodeType.IsCast())
        {
            expression = expression.GetUnaryOperand();
        }

        if (!expression.NodeType.IsBinary())
        {
            yield return expression;
            yield break;
        }

        var binary = (BinaryExpression)expression;

        if (binary.NodeType != Add)
        {
            yield return expression;
            yield break;
        }

        foreach (var operand in FlattenOperands(binary))
        {
            yield return operand;
        }
    }

    public static bool TryCreateForConcatCall(
        MethodCallExpression methodCall,
        ITranslationContext context,
        out INodeTranslation concatTranslation)
    {
        if (context.Settings.ShowStringConcatMethodCalls ||
           !IsStringConcatCall(methodCall))
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

        concatTranslation = new StringConcatenationTranslation(
            Call,
            operandCount,
            operands,
            context);

        return true;
    }

    private static bool IsStringConcatCall(MethodCallExpression methodCall)
    {
        var method = methodCall.Method;

        return method.IsStatic &&
               method.DeclaringType == typeof(string) &&
               method.Name == nameof(string.Concat) &&
               AreAllStrings(method.GetParameters());
    }

    private static bool AreAllStrings(IList<ParameterInfo> parameters)
    {
        if (parameters.Count == 1 &&
            parameters[0].ParameterType.TryGetElementType(out var elementType) &&
            elementType == typeof(string))
        {
            return true;
        }

        return parameters.All(p => p.ParameterType == typeof(string));
    }

    #endregion

    public ExpressionType NodeType { get; }

    public int TranslationLength => 
        _operandTranslations.TotalTranslationLength(separator: " + ");

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

    private enum OperandType { String, ToStringCall, NonString }
}
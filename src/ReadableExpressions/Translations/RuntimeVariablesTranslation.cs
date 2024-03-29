﻿namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal static class RuntimeVariablesTranslation
{
    public static INodeTranslation For(
        RuntimeVariablesExpression runtimeVariables,
        ITranslationContext context)
    {
        return ParameterSetTranslation
            .For(runtimeVariables.Variables, context)
            .WithNodeType(ExpressionType.RuntimeVariables);
    }
}
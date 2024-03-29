﻿namespace AgileObjects.ReadableExpressions.Translations;

using System.Globalization;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

internal static class DebugInfoTranslation
{
    public static INodeTranslation For(
        DebugInfoExpression debugInfo, 
        ITranslationContext context)
    {
        string debugInfoText;

        if (debugInfo.IsClear)
        {
            debugInfoText = "Clear debug info from " + debugInfo.Document.FileName;
        }
        else
        {
            debugInfoText = string.Format(
                CultureInfo.InvariantCulture,
                "Debug to {0}, {1}, {2} -> {3}, {4}",
                debugInfo.Document.FileName,
                debugInfo.StartLine,
                debugInfo.StartColumn,
                debugInfo.EndLine,
                debugInfo.EndColumn);
        }

        return context
            .GetTranslationFor(ReadableExpression.Comment(debugInfoText))
            .WithNodeType(ExpressionType.DebugInfo);
    }
}
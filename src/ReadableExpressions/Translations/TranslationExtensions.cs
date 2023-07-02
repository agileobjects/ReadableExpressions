namespace AgileObjects.ReadableExpressions.Translations;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;
using Formatting;
using static TranslationConstants;

internal static class TranslationExtensions
{
    public static bool IsMultiStatement(this INodeTranslation translation)
    {
        switch (translation.NodeType)
        {
            case ExpressionType.Call:
            case ExpressionType.MemberAccess:
            case ExpressionType.Parameter:
                return false;
        }

        return translation is IPotentialMultiStatementTranslatable { IsMultiStatement: true };
    }

    public static bool IsTerminated(this ITranslation translation)
        => translation is IPotentialSelfTerminatingTranslation { IsTerminated: true };

    public static bool HasGoto(this ITranslation translation)
        => translation is IPotentialGotoTranslation { HasGoto: true };

    public static bool IsParenthesized(this ITranslation translation)
        => translation is IPotentialParenthesizedTranslation { Parenthesize: true };

    public static bool WrapLine(this ITranslation translation)
        => translation.TranslationLength > LineWrapThreshold;

    public static bool IsAssignment(this INodeTranslation translation)
        => AssignmentTranslation.IsAssignment(translation.NodeType);

    public static bool IsBinary(this ExpressionType nodeType)
        => BinaryTranslation.IsBinary(nodeType);

    public static bool IsCast(this INodeTranslation translation)
        => translation.NodeType.IsCast();

    public static bool IsCast(this ExpressionType nodeType)
        => CastTranslation.IsCast(nodeType);

    public static INodeTranslation WithParentheses(
        this INodeTranslation translation)
    {
        return new WrappedTranslation("(", translation, ")");
    }

    public static int TotalTranslationLength<TTranslatable>(
        this IList<TTranslatable> translatables,
        string separator = null)
        where TTranslatable : ITranslation
    {
        var count = translatables.Count;
        separator ??= string.Empty;

        switch (count)
        {
            case 0:
                return 0;

            case 1:
                return translatables[0].TranslationLength;

            case 2:
                return
                    translatables[0].TranslationLength +
                    separator.Length +
                    translatables[1].TranslationLength;

            default:
                var translationLength = translatables[0].TranslationLength;

                for (var i = 1; i < count; ++i)
                {
                    translationLength +=
                        separator.Length +
                        translatables[i].TranslationLength;
                }

                return translationLength;
        }
    }

    public static INodeTranslation WithNodeType(
        this ITranslation translation,
        ExpressionType nodeType)
    {
        return new TranslationAdapter(translation, nodeType);
    }

    public static void WriteInParentheses(
        this ITranslation translation,
        TranslationWriter writer)
    {
        writer.WriteToTranslation('(');
        translation.WriteTo(writer);
        writer.WriteToTranslation(')');
    }

    public static void WriteInParenthesesIfRequired(
        this INodeTranslation nestedTranslation,
        TranslationWriter writer,
        ITranslationContext context)
    {
        if (ShouldWriteInParentheses(nestedTranslation))
        {
            nestedTranslation.WriteInParentheses(writer);
            return;
        }

        new CodeBlockTranslation(nestedTranslation, context).WriteTo(writer);
    }

    public static bool ShouldWriteInParentheses(
        this INodeTranslation nestedTranslation)
    {
        if (nestedTranslation.NodeType == ExpressionType.Conditional ||
            nestedTranslation.IsAssignment() || nestedTranslation.IsCast())
        {
            return true;
        }

        if (nestedTranslation is IPotentialParenthesizedTranslation parenthesized)
        {
            return parenthesized.Parenthesize;
        }

        return nestedTranslation.NodeType.IsBinary();
    }

    public static void WriteNewToTranslation(this TranslationWriter writer)
        => writer.WriteKeywordToTranslation("new ");

    public static void WriteReturnToTranslation(this TranslationWriter writer)
        => writer.WriteControlStatementToTranslation("return ");

    public static void WriteControlStatementToTranslation(
        this TranslationWriter writer,
        string statement)
    {
        writer.WriteToTranslation(statement, TokenType.ControlStatement);
    }

    public static void WriteSemiColonToTranslation(
        this TranslationWriter writer)
    {
        writer.WriteToTranslation(';');
    }
}
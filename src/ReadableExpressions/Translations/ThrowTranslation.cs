namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class ThrowTranslation : INodeTranslation, IPotentialGotoTranslation
{
    private const string _throw = "throw";
    private readonly INodeTranslation _thrownItemTranslation;

    public ThrowTranslation(
        UnaryExpression throwExpression,
        ITranslationContext context)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // unary.Operand is null when using Expression.Rethrow():
        if (throwExpression.Operand == null ||
            context.Analysis.IsCatchBlockVariable(throwExpression.Operand))
        {
            return;
        }

        _thrownItemTranslation = context.GetTranslationFor(throwExpression.Operand);
    }

    public ExpressionType NodeType => ExpressionType.Throw;

    public int TranslationLength
        => _throw.Length + (_thrownItemTranslation?.TranslationLength ?? 0);

    public bool HasGoto => true;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteKeywordToTranslation(_throw);

        if (_thrownItemTranslation == null)
        {
            return;
        }

        writer.WriteSpaceToTranslation();
        _thrownItemTranslation.WriteTo(writer);
    }
}
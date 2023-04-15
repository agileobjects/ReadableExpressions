namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class LoopTranslation : INodeTranslation, IPotentialSelfTerminatingTranslation
{
    private readonly INodeTranslation _loopBodyTranslation;

    public LoopTranslation(LoopExpression loop, ITranslationContext context)
    {
        _loopBodyTranslation = context
            .GetCodeBlockTranslationFor(loop.Body)
            .WithTermination()
            .WithBraces();
    }

    public ExpressionType NodeType => ExpressionType.Loop;

    public int TranslationLength => _loopBodyTranslation.TranslationLength + 10;

    public bool IsTerminated => true;

    public void WriteTo(TranslationWriter writer)
    {
        writer.WriteControlStatementToTranslation("while ");
        writer.WriteToTranslation('(');
        writer.WriteKeywordToTranslation("true");
        writer.WriteToTranslation(')');
        _loopBodyTranslation.WriteTo(writer);
    }
}
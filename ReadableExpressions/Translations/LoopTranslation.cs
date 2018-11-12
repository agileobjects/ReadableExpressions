namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class LoopTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslation _loopBodyTranslation;

        public LoopTranslation(LoopExpression loop, ITranslationContext context)
        {
            _loopBodyTranslation = context.GetCodeBlockTranslationFor(loop.Body).WithTermination().WithBraces();
            EstimatedSize = _loopBodyTranslation.EstimatedSize + 10;
        }

        public ExpressionType NodeType => ExpressionType.Loop;

        public int EstimatedSize { get; }

        public bool IsTerminated => true;

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation("while (true)");
            _loopBodyTranslation.WriteTo(context);
        }
    }
}
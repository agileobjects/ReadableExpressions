namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class LoopTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslation _loopBodyTranslation;

        public LoopTranslation(LoopExpression loop, ITranslationContext context)
        {
            Type = loop.Type;
            _loopBodyTranslation = context.GetCodeBlockTranslationFor(loop.Body).WithTermination().WithBraces();
            TranslationSize = _loopBodyTranslation.TranslationSize + 10;

            FormattingSize =
                 context.GetControlStatementFormattingSize() + // <- for 'while'
                 context.GetKeywordFormattingSize() + // <- for 'true'
                _loopBodyTranslation.FormattingSize;
        }

        public ExpressionType NodeType => ExpressionType.Loop;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsTerminated => true;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteControlStatementToTranslation("while ");
            buffer.WriteToTranslation('(');
            buffer.WriteKeywordToTranslation("true");
            buffer.WriteToTranslation(')');
            _loopBodyTranslation.WriteTo(buffer);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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

        public int GetIndentSize() => _loopBodyTranslation.GetIndentSize();

        public int GetLineCount() => _loopBodyTranslation.GetLineCount() + 1;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteControlStatementToTranslation("while ");
            writer.WriteToTranslation('(');
            writer.WriteKeywordToTranslation("true");
            writer.WriteToTranslation(')');
            _loopBodyTranslation.WriteTo(writer);
        }
    }
}
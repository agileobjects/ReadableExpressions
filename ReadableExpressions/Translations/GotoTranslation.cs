namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal static class GotoTranslation
    {
        public static ITranslation For(GotoExpression @goto, ITranslationContext context)
        {
            switch (@goto.Kind)
            {
                case GotoExpressionKind.Break:
                    return new TerminatedGotoTranslation(@goto, "break", context);

                case GotoExpressionKind.Continue:
                    return new TerminatedGotoTranslation(@goto, "continue", context);

                case GotoExpressionKind.Return:
                    if (@goto.Value == null)
                    {
                        return new TerminatedGotoTranslation(@goto, "return", context);
                    }

                    return new ReturnValueTranslation(@goto, context);

                case GotoExpressionKind.Goto when context.GoesToReturnLabel(@goto):
                    goto case GotoExpressionKind.Return;

                default:
                    return new GotoNamedLabelTranslation(@goto, context);
            }
        }

        private class TerminatedGotoTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
        {
            private readonly string _statement;

            public TerminatedGotoTranslation(
                Expression @goto,
                string statement,
                ITranslationContext context)
            {
                Type = @goto.Type;
                _statement = statement;
                TranslationSize = statement.Length + ";".Length;
                FormattingSize = context.GetControlStatementFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.Goto;

            public Type Type { get; }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public bool IsTerminated => true;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteControlStatementToTranslation(_statement);
                buffer.WriteToTranslation(';');
            }
        }

        private class GotoNamedLabelTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
        {
            private readonly string _labelName;

            public GotoNamedLabelTranslation(GotoExpression @goto, ITranslationContext context)
            {
                Type = @goto.Type;
                _labelName = @goto.Target.Name;
                TranslationSize = "goto ".Length + _labelName.Length + ";".Length;
                FormattingSize = context.GetControlStatementFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.Goto;

            public Type Type { get; }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public bool IsTerminated => true;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteControlStatementToTranslation("goto ");
                buffer.WriteToTranslation(_labelName);
                buffer.WriteToTranslation(';');
            }
        }

        private class ReturnValueTranslation : ITranslation
        {
            private readonly CodeBlockTranslation _returnValueTranslation;

            public ReturnValueTranslation(GotoExpression @goto, ITranslationContext context)
            {
                _returnValueTranslation = context.GetCodeBlockTranslationFor(@goto.Value);
                TranslationSize = _returnValueTranslation.TranslationSize + "return ".Length;
                FormattingSize = context.GetControlStatementFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.Goto;

            public Type Type => _returnValueTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetLineCount() => _returnValueTranslation.GetLineCount();

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteReturnToTranslation();
                _returnValueTranslation.WriteTo(buffer);
            }
        }
    }
}
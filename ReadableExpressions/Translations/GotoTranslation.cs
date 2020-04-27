﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;
    using Interfaces;

    internal static class GotoTranslation
    {
        public static ITranslation For(GotoExpression @goto, ITranslationContext context)
        {
            switch (@goto.Kind)
            {
                case GotoExpressionKind.Break:
                    return new TerminatedGotoTranslation(@goto, "break");

                case GotoExpressionKind.Continue:
                    return new TerminatedGotoTranslation(@goto, "continue");

                case GotoExpressionKind.Return:
                    if (@goto.Value == null)
                    {
                        return new TerminatedGotoTranslation(@goto, "return");
                    }

                    return new ReturnValueTranslation(@goto, context);

                case GotoExpressionKind.Goto when context.GoesToReturnLabel(@goto):
                    goto case GotoExpressionKind.Return;

                default:
                    return FixedTerminatedValueTranslation("goto " + @goto.Target.Name + ";", @goto);
            }
        }

        private static FixedTerminatedValueTranslation FixedTerminatedValueTranslation(string value, GotoExpression @goto)
            => new FixedTerminatedValueTranslation(ExpressionType.Goto, value, @goto.Type, TokenType.ControlStatement);

        private class TerminatedGotoTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
        {
            private readonly string _statement;

            public TerminatedGotoTranslation(Expression @goto, string statement)
            {
                Type = @goto.Type;
                _statement = statement;
                EstimatedSize = _statement.Length + 1;
            }

            public ExpressionType NodeType => ExpressionType.Goto;

            public Type Type {get;}

            public int EstimatedSize { get; }

            public bool IsTerminated => true;

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteControlStatementToTranslation(_statement);
                buffer.WriteToTranslation(';');
            }
        }

        private class ReturnValueTranslation : ITranslation
        {
            private const string _returnKeyword = "return ";
            private readonly CodeBlockTranslation _returnValueTranslation;

            public ReturnValueTranslation(GotoExpression @goto, ITranslationContext context)
            {
                _returnValueTranslation = context.GetCodeBlockTranslationFor(@goto.Value);
                EstimatedSize = _returnValueTranslation.EstimatedSize + _returnKeyword.Length;
            }

            public ExpressionType NodeType => ExpressionType.Goto;

            public Type Type => _returnValueTranslation.Type;

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteControlStatementToTranslation(_returnKeyword);
                _returnValueTranslation.WriteTo(buffer);
            }
        }
    }
}
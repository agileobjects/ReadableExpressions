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
                    return FixedTerminatedValueTranslation("break;", @goto);

                case GotoExpressionKind.Continue:
                    return FixedTerminatedValueTranslation("continue;", @goto);

                case GotoExpressionKind.Return:
                    if (@goto.Value == null)
                    {
                        return FixedTerminatedValueTranslation("return;", @goto);
                    }

                    return new ReturnValueTranslation(@goto, context);

                case GotoExpressionKind.Goto when context.GoesToReturnLabel(@goto):
                    goto case GotoExpressionKind.Return;

                default:
                    return FixedTerminatedValueTranslation("goto " + @goto.Target.Name + ";", @goto);
            }
        }

        private static FixedTerminatedValueTranslation FixedTerminatedValueTranslation(string value, GotoExpression @goto)
            => new FixedTerminatedValueTranslation(ExpressionType.Goto, value, @goto.Type);

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
                buffer.WriteToTranslation(_returnKeyword);
                _returnValueTranslation.WriteTo(buffer);
            }
        }
    }
}
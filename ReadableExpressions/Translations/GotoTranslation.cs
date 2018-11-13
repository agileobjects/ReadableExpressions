namespace AgileObjects.ReadableExpressions.Translations
{
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
                    return new FixedTerminatedValueTranslation(ExpressionType.Goto, "break;");

                case GotoExpressionKind.Continue:
                    return new FixedTerminatedValueTranslation(ExpressionType.Goto, "continue;");

                case GotoExpressionKind.Return:
                    if (@goto.Value == null)
                    {
                        return new FixedTerminatedValueTranslation(ExpressionType.Goto, "return;");
                    }

                    return new ReturnValueTranslation(@goto, context);

                case GotoExpressionKind.Goto when context.GoesToReturnLabel(@goto):
                    goto case GotoExpressionKind.Return;

                default:
                    return new FixedTerminatedValueTranslation(ExpressionType.Goto, "goto " + @goto.Target.Name + ";");
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

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation(_returnKeyword);
                _returnValueTranslation.WriteTo(context);
            }
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class GotoExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<GotoExpressionKind, Func<GotoExpression, TranslationContext, string>> _gotoKindHandlers;

        public GotoExpressionTranslator()
            : base(ExpressionType.Goto)
        {
            _gotoKindHandlers = new Dictionary<GotoExpressionKind, Func<GotoExpression, TranslationContext, string>>
            {
                [GotoExpressionKind.Break] = TranslateBreak,
                [GotoExpressionKind.Continue] = TranslateContinue,
                [GotoExpressionKind.Goto] = TranslateGoto,
                [GotoExpressionKind.Return] = TranslateReturn,
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var gotoExpression = (GotoExpression)expression;

            return _gotoKindHandlers[gotoExpression.Kind].Invoke(gotoExpression, context);
        }

        private static string TranslateBreak(GotoExpression breakExpression, TranslationContext context)
            => "break;";

        private static string TranslateContinue(GotoExpression continueExpression, TranslationContext context)
            => "continue;";

        private static string TranslateGoto(GotoExpression @goto, TranslationContext context)
        {
            if (context.GoesToReturnLabel(@goto))
            {
                return TranslateReturn(@goto, context);
            }

            return $"goto {@goto.Target.Name};";
        }

        private static string TranslateReturn(GotoExpression returnExpression, TranslationContext context)
        {
            if (returnExpression.Value == null)
            {
                return "return;";
            }

            var value = context.TranslateAsCodeBlock(returnExpression.Value);

            return $"return {value}";
        }
    }
}
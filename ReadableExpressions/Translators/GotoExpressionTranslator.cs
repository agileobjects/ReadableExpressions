namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class GotoExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<GotoExpressionKind, Func<GotoExpression, TranslationContext, string>> _gotoKindHandlers;

        public GotoExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Goto)
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

        private static string TranslateBreak(GotoExpression gotoExpression, TranslationContext context)
            => "break;";

        private static string TranslateContinue(GotoExpression gotoExpression, TranslationContext context)
            => "continue;";

        private static string TranslateGoto(GotoExpression gotoExpression, TranslationContext context)
            => $"goto {gotoExpression.Target.Name};";

        private string TranslateReturn(GotoExpression gotoExpression, TranslationContext context)
        {
            if (gotoExpression.Value == null)
            {
                return "return;";
            }

            var value = GetTranslation(gotoExpression.Value, context);

            return $"return {value}";
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class GotoExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<GotoExpressionKind, Func<GotoExpression, IExpressionTranslatorRegistry, string>>
            _gotoKindHandlers = new Dictionary<GotoExpressionKind, Func<GotoExpression, IExpressionTranslatorRegistry, string>>
            {
                [GotoExpressionKind.Break] = TranslateBreak,
                [GotoExpressionKind.Continue] = TranslateContinue,
                [GotoExpressionKind.Goto] = TranslateGoto,
                [GotoExpressionKind.Return] = TranslateReturn,
            };

        public GotoExpressionTranslator()
            : base(ExpressionType.Goto)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var gotoExpression = (GotoExpression)expression;

            return _gotoKindHandlers[gotoExpression.Kind].Invoke(gotoExpression, translatorRegistry);
        }

        private static string TranslateBreak(
            GotoExpression gotoExpression,
            IExpressionTranslatorRegistry translatorRegistry) => "break;";

        private static string TranslateContinue(
            GotoExpression gotoExpression,
            IExpressionTranslatorRegistry translatorRegistry) => "continue;";


        private static string TranslateGoto(
            GotoExpression gotoExpression,
            IExpressionTranslatorRegistry translatorRegistry) => $"goto {gotoExpression.Target.Name};";

        private static string TranslateReturn(GotoExpression gotoExpression, IExpressionTranslatorRegistry translatorRegistry)
        {
            if (gotoExpression.Value == null)
            {
                return "return;";
            }

            var value = translatorRegistry.Translate(gotoExpression.Value);

            return $"return {value}";
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal class GotoExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<GotoExpressionKind, Func<GotoExpression, string>> _gotoKindHandlers;

        public GotoExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Goto)
        {
            _gotoKindHandlers = new Dictionary<GotoExpressionKind, Func<GotoExpression, string>>
            {
                [GotoExpressionKind.Break] = TranslateBreak,
                [GotoExpressionKind.Continue] = TranslateContinue,
                [GotoExpressionKind.Goto] = TranslateGoto,
                [GotoExpressionKind.Return] = TranslateReturn,
            };
        }

        public override string Translate(Expression expression)
        {
            var gotoExpression = (GotoExpression)expression;

            return _gotoKindHandlers[gotoExpression.Kind].Invoke(gotoExpression);
        }

        private static string TranslateBreak(GotoExpression gotoExpression) => "break;";

        private static string TranslateContinue(GotoExpression gotoExpression) => "continue;";

        private static string TranslateGoto(GotoExpression gotoExpression) => $"goto {gotoExpression.Target.Name};";

        private string TranslateReturn(GotoExpression gotoExpression)
        {
            if (gotoExpression.Value == null)
            {
                return "return;";
            }

            var value = Registry.Translate(gotoExpression.Value);

            return $"return {value}";
        }
    }
}
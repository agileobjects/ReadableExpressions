namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using GotoExpression = Microsoft.Scripting.Ast.GotoExpression;
    using GotoExpressionKind = Microsoft.Scripting.Ast.GotoExpressionKind;
#endif

    internal struct GotoExpressionTranslator : IExpressionTranslator
    {
        private static readonly Dictionary<GotoExpressionKind, Func<GotoExpression, TranslationContext, string>> _gotoKindHandlers =
            new Dictionary<GotoExpressionKind, Func<GotoExpression, TranslationContext, string>>
            {
                [GotoExpressionKind.Break] = TranslateBreak,
                [GotoExpressionKind.Continue] = TranslateContinue,
                [GotoExpressionKind.Goto] = TranslateGoto,
                [GotoExpressionKind.Return] = TranslateReturn,
            };

        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Goto; }
        }

        public string Translate(Expression expression, TranslationContext context)
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
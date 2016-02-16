namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class LoopExpressionTranslator : ExpressionTranslatorBase
    {
        public LoopExpressionTranslator()
            : base(ExpressionType.Loop)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var loop = (LoopExpression)expression;

            var loopBody = translatorRegistry
                .TranslateExpressionBody(loop.Body, loop.Type);

            var translated = $@"
while (true){loopBody}";
            return translated.TrimStart();
        }
    }
}
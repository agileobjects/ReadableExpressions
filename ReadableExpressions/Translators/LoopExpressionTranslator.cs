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

            var loopBodyBlock = translatorRegistry
                .TranslateExpressionBody(loop.Body, loop.Type);

            return $"while (true){loopBodyBlock.WithBrackets()}";
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class LoopExpressionTranslator : ExpressionTranslatorBase
    {
        public LoopExpressionTranslator()
            : base(ExpressionType.Loop)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var loop = (LoopExpression)expression;

            var loopBodyBlock = context.TranslateCodeBlock(loop.Body);

            return $"while (true){loopBodyBlock.WithCurlyBraces()}";
        }
    }
}
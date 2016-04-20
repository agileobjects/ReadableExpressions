namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LoopExpressionTranslator : ExpressionTranslatorBase
    {
        public LoopExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Loop)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var loop = (LoopExpression)expression;

            var loopBodyBlock = GetTranslatedExpressionBody(loop.Body, context);

            return $"while (true){loopBodyBlock.WithParentheses()}";
        }
    }
}
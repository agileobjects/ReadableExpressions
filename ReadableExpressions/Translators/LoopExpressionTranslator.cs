namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LoopExpressionTranslator : ExpressionTranslatorBase
    {
        public LoopExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Loop)
        {
        }

        public override string Translate(Expression expression)
        {
            var loop = (LoopExpression)expression;

            var loopBodyBlock = GetTranslatedExpressionBody(loop.Body);

            return $"while (true){loopBodyBlock.WithBrackets()}";
        }
    }
}
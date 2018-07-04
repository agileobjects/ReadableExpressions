namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using LoopExpression = Microsoft.Scripting.Ast.LoopExpression;

#endif

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
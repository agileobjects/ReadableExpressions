namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using LoopExpression = Microsoft.Scripting.Ast.LoopExpression;
#endif

    internal struct LoopExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Loop; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var loop = (LoopExpression)expression;

            var loopBodyBlock = context.TranslateCodeBlock(loop.Body);

            return $"while (true){loopBodyBlock.WithCurlyBraces()}";
        }
    }
}
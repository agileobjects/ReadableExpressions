namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif

    internal struct QuotedLambdaExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Quote; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var quote = (UnaryExpression)expression;

            if (context.Settings.DoNotCommentQuotedLambdas)
            {
                return context.TranslateAsCodeBlock(quote.Operand);
            }

            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quote.Operand);

            var translatedLambda = context
                .TranslateCodeBlock(quotedLambdaBlock)
                .Indented()
                .WithoutCurlyBraces();

            return Environment.NewLine + translatedLambda;
        }
    }
}
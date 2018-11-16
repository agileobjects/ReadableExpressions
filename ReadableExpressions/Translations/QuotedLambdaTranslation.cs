namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class QuotedLambdaTranslation : ITranslation
    {
        private readonly ITranslation _quotedLambdaTranslation;

        private QuotedLambdaTranslation(UnaryExpression quotedLambda, ITranslationContext context)
        {
            var comment = ReadableExpression.Comment("Quoted to induce a closure:");
            var quotedLambdaBlock = Expression.Block(comment, quotedLambda.Operand);

            _quotedLambdaTranslation = context.GetTranslationFor(quotedLambdaBlock);
        }

        public static ITranslation For(UnaryExpression quotedLambda, ITranslationContext context)
        {
            if (context.Settings.DoNotCommentQuotedLambdas)
            {
                return context.GetCodeBlockTranslationFor(quotedLambda.Operand).WithNodeType(ExpressionType.Quote);
            }

            return new QuotedLambdaTranslation(quotedLambda, context);
        }

        public ExpressionType NodeType => ExpressionType.Quote;

        public Type Type => _quotedLambdaTranslation.Type;

        public int EstimatedSize => _quotedLambdaTranslation.EstimatedSize;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteNewLineToTranslation();
            _quotedLambdaTranslation.WriteTo(buffer);
        }
    }
}
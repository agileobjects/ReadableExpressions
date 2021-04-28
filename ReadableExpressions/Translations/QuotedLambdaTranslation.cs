namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

        public int TranslationSize => _quotedLambdaTranslation.TranslationSize;

        public int FormattingSize => _quotedLambdaTranslation.FormattingSize;

        public int GetIndentSize() => _quotedLambdaTranslation.GetIndentSize();

        public int GetLineCount() => _quotedLambdaTranslation.GetLineCount() + 1;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteNewLineToTranslation();
            _quotedLambdaTranslation.WriteTo(writer);
        }
    }
}
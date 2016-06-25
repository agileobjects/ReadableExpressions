namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Formatting;

    public delegate string Translator(Expression expression, TranslationContext context);

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
        }

        public IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

        public abstract string Translate(Expression expression, TranslationContext context);

        protected ParameterSet GetTranslatedParameters(
            IEnumerable<Expression> parameters,
            TranslationContext context,
            IMethodInfo method = null)
        {
            return new ParameterSet(method, parameters, context);
        }

        protected CodeBlock GetTranslatedExpressionBody(
            Expression body,
            TranslationContext context)
        {
            var codeBlock = TranslateBlock(body as BlockExpression, context) ?? TranslateSingle(body, context);

            return codeBlock;
        }

        #region TranslateExpressionBody

        private static CodeBlock TranslateBlock(BlockExpression bodyBlock, TranslationContext context)
        {
            if (bodyBlock == null)
            {
                return null;
            }

            if (bodyBlock.Expressions.Count == 1)
            {
                return TranslateSingle(bodyBlock, context);
            }

            var block = context.GetTranslation(bodyBlock);
            var blockLines = block.SplitToLines();

            return new CodeBlock(bodyBlock, blockLines);
        }

        private static CodeBlock TranslateSingle(Expression bodySingle, TranslationContext context)
        {
            var body = context.GetTranslation(bodySingle).WithoutSurroundingParentheses(bodySingle);

            return new CodeBlock(bodySingle, body);
        }

        #endregion
    }
}
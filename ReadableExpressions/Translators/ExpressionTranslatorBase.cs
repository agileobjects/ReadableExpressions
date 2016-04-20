namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Formatting;

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly Func<Expression, TranslationContext, string> _globalTranslator;
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(
            Func<Expression, TranslationContext, string> globalTranslator,
            params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
            _globalTranslator = globalTranslator;
        }

        public IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

        public abstract string Translate(Expression expression, TranslationContext context);

        protected string GetTranslation(Expression expression, TranslationContext context)
        {
            return _globalTranslator.Invoke(expression, context);
        }

        protected ParameterSet GetTranslatedParameters(
            IEnumerable<Expression> parameters,
            TranslationContext context,
            IMethodInfo method = null)
        {
            return new ParameterSet(method, parameters, context, _globalTranslator);
        }

        protected CodeBlock GetTranslatedExpressionBody(
            Expression body,
            TranslationContext context)
        {
            var codeBlock = TranslateBlock(body as BlockExpression, context) ?? TranslateSingle(body, context);

            return codeBlock;
        }

        #region TranslateExpressionBody

        private CodeBlock TranslateBlock(BlockExpression bodyBlock, TranslationContext context)
        {
            if (bodyBlock == null)
            {
                return null;
            }

            if (bodyBlock.Expressions.Count == 1)
            {
                return TranslateSingle(bodyBlock, context);
            }

            var block = GetTranslation(bodyBlock, context);
            var blockLines = block.SplitToLines();

            return new CodeBlock(blockLines);
        }

        private CodeBlock TranslateSingle(Expression bodySingle, TranslationContext context)
        {
            var body = GetTranslation(bodySingle, context).WithoutSurroundingParentheses(bodySingle);

            return new CodeBlock(body);
        }

        #endregion
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Formatting;

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly Func<Expression, string> _globalTranslator;
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(
            Func<Expression, string> globalTranslator,
            params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
            _globalTranslator = globalTranslator;
        }

        public IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

        public abstract string Translate(Expression expression);

        protected string GetTranslation(Expression expression)
        {
            return _globalTranslator.Invoke(expression);
        }

        protected ParameterSet GetTranslatedParameters(IEnumerable<Expression> parameters)
        {
            return new ParameterSet(parameters, _globalTranslator);
        }

        protected CodeBlock GetTranslatedExpressionBody(Expression body)
        {
            var codeBlock = TranslateBlock(body as BlockExpression) ?? TranslateSingle(body);

            return codeBlock;
        }

        #region TranslateExpressionBody

        private CodeBlock TranslateBlock(BlockExpression bodyBlock)
        {
            if (bodyBlock == null)
            {
                return null;
            }

            if (bodyBlock.Expressions.Count == 1)
            {
                return TranslateSingle(bodyBlock);
            }

            var block = GetTranslation(bodyBlock);
            var blockLines = block.SplitToLines();

            return new CodeBlock(blockLines);
        }

        private CodeBlock TranslateSingle(Expression bodySingle)
        {
            var body = GetTranslation(bodySingle);

            if ((body?.StartsWith("(", StringComparison.Ordinal) == true) &&
                body.EndsWith(")", StringComparison.Ordinal))
            {
                body = body.Substring(1, body.Length - 2);
            }

            return new CodeBlock(body);
        }

        #endregion
    }
}
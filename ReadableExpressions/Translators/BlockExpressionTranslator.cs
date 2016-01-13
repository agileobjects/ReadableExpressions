namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        public BlockExpressionTranslator()
            : base(ExpressionType.Block)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var block = (BlockExpression)expression;

            var expressions = block
                .Expressions
                .Select(translatorRegistry.Translate)
                .Where(t => t != null)
                .Select(t => t + ";")
                .ToArray();

            return string.Join(Environment.NewLine, expressions);
        }
    }
}
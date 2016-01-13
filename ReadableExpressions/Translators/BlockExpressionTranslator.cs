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
            var expressions = block.Expressions.Select(exp => translatorRegistry.Translate(exp) + ";").ToArray();

            if (block.Type != typeof(void))
            {
                expressions[expressions.Length - 1] = "return " + expressions[expressions.Length - 1];
            }

            return string.Join(Environment.NewLine, expressions);
        }
    }
}
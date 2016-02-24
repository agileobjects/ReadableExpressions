namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class NegationExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, string> _negationsByNodeType = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Not] = "!",
            [ExpressionType.Negate] = "-"
        };

        internal NegationExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, _negationsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression)
        {
            var negation = (UnaryExpression)expression;

            return _negationsByNodeType[expression.NodeType] + Registry.Translate(negation.Operand);
        }
    }
}
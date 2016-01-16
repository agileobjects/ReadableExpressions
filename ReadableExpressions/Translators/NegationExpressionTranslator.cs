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

        internal NegationExpressionTranslator()
            : base(_negationsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var negation = (UnaryExpression)expression;

            return _negationsByNodeType[expression.NodeType] + translatorRegistry.Translate(negation.Operand);
        }
    }
}
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class NegationExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, string> _negationsByNodeType = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Not] = "!",
            [ExpressionType.Negate] = "-",
            [ExpressionType.NegateChecked] = "-"
        };

        internal NegationExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, _negationsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression)
        {
            var negation = (UnaryExpression)expression;

            return _negationsByNodeType[expression.NodeType] + GetTranslation(negation.Operand);
        }
    }
}
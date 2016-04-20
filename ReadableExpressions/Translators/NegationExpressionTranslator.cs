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

        internal NegationExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, _negationsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var negation = (UnaryExpression)expression;

            return Translate(expression.NodeType, negation.Operand, context);
        }

        public string TranslateNot(Expression expression, TranslationContext context)
        {
            return Translate(ExpressionType.Not, expression, context);
        }

        private string Translate(ExpressionType negationType, Expression expression, TranslationContext context)
        {
            return _negationsByNodeType[negationType] + GetTranslation(expression, context);
        }
    }
}
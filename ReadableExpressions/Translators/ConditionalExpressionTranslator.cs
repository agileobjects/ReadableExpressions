namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class ConditionalExpressionTranslator : ExpressionTranslatorBase
    {
        public ConditionalExpressionTranslator()
            : base(ExpressionType.Conditional)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var conditional = (ConditionalExpression)expression;

            var test = translatorRegistry.Translate(conditional.Test);
            var ifTrue = translatorRegistry.Translate(conditional.IfTrue);
            var ifFalse = translatorRegistry.Translate(conditional.IfFalse);

            return $"{test} ? {ifTrue} : {ifFalse}";
        }
    }
}
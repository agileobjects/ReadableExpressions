namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        internal CastExpressionTranslator()
            : base(ExpressionType.Convert)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var conversion = (UnaryExpression)expression;
            var conversionSubject = translatorRegistry.Translate(conversion.Operand);

            if (expression.Type == typeof(object))
            {
                // Don't bother showing a boxing operation:
                return conversionSubject;
            }

            return "(" + conversion.Type.Name + ")" + conversionSubject;
        }
    }
}